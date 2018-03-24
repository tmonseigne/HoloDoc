
const cv = require('opencv4nodejs');
const utils = require('./improc-utils');
const reco = require('./improc-recognition')

exports.MAX_FEATURES_DISTANCE = reco.MAX_FEATURES_DISTANCE;
exports.HIST_BINS = reco.HIST_BINS;

/**
 * Detect all Documents of an image
 * @param {cv.Mat} image Opencv4NodeJS Mat
 * @param {Array.<Number>} backgroundColor Array of three Number
 * @returns {Array.<Array.<cv.Point>>} Array of Array of Opencv4NodeJS points represented all docs in image
 */
exports.detectDocuments = function (image, backgroundColor = [25,25,25]) {
	let result = [];
	if(!image)	return result;

	image = image.cvtColor(cv.COLOR_BGR2GRAY);
	image = image.gaussianBlur(new cv.Size(3,3), 0);

	let thresholdResult = image.canny(50, 155);
	thresholdResult = thresholdResult.dilate(new cv.Mat(3,3, cv.CV_8U, 1));

	let width = image.cols;
	let height = image.rows;
	let minLength = 0.2 * (width + height);
	let maxLength = 1.4 * (width + height);

	cv.imwrite('./seuillage.jpg', thresholdResult);

	let contours = thresholdResult.findContours(cv.RETR_LIST, cv.CHAIN_APPROX_SIMPLE);
	let i = 0;
	for (let c of contours) {
		let perimeter = c.arcLength(true);

		if (c.numPoints >= 4 && minLength <= perimeter && perimeter <= maxLength) {
			let corners = utils.extractCorners(c, minLength, maxLength);
			if (corners != undefined) {
				if (utils.verifyShape(corners)) {
					result.push(corners);
				}
			}
		}
		i++;
	}

	result = result.sort(utils.getEstimatedArea);

	for (let i = 0; i < result.length; ++i) {
		let points = result[i];
		for (let j = i + 1; j < result.length; ++j) {
			if (utils.inQuad(points, utils.getCentroid(result[j]))) {
				result.splice(j, 1);
				j--;
			}
		}
	}

	return result;
};

/**
 * Get the coordinates of the center of the image
 * @param {cv.Mat} image Image
 * @returns {cv.Point} The coordinates of the center of the image
 */
exports.getCenter = function (image) {
	return utils.getCenter(image);
}

/**
 * Get the Array of Opencv4NodeJS points where the centroid is closest to the point passed in parameter
 * @param {Array.<Array.<cv.Point>>} docs Array of Array of Opencv4NodeJS points
 * @param {cv.Point} from Opencv4NodeJS points
 * @returns {Array.<cv.Point>} Array of Opencv4NodeJS points selected
 */
exports.getNearestdocFrom = function (docs, from) {
	return utils.getNearestdocFrom(docs, from);
};

/**
 * Extract all features
 * @param {cv.Mat} image Image in BGR
 * @param {Number} bins Number of bins in the Histogramms
 * @returns {Array.<Array.<Number>>} Features represented by six rows and N columns
 */
exports.extractFeatures = function (image, bins = reco.HIST_BINS) {
	return reco.extractFeatures(image, bins);
};

/**
 * Compare two set of features with weight
 * @param {Array.<Array.<Number>>} features1 Features One
 * @param {Array.<Array.<Number>>} features2 Features Two
 * @param {Array.<Number>} coefs Weight (Array of Six Numbers) => (H,S,V,B,G,R)
 * @returns {Number} Weighted Mean Distance of Six Features
 */
exports.featuresDistance = function (features1, features2, coefs = reco.HIST_COEFS) {
	return reco.featuresDistance(features1, features2, coefs);
}

/**
 * Normalize Distance by the maximum distance possible
 * @param {Number} distance Distance to normalized
 * @returns {Number} Distance Normalized
 */
exports.featureDistanceNormalization = function (distance) {
	return reco.featureDistanceNormalization(distance);
}

/**
 * Crop the quad and correct the perspective
 * @param {cv.Mat} image Opencv4NodeJS Mat represent image
 * @param {Array.<cv.Point>} doc Array of four Opencv4NodeJS points represent the doc to extract
 * @returns {cv.Mat} The undeformed quad
 */
exports.undistordDoc = function (image, doc) {
	if (!image || !doc) return undefined;
	let newOrder = utils.sortDocCorners(doc); // illuminatus es spiritus sancti

	let w = Math.max(utils.distBetweenPoints(newOrder[0], newOrder[1]), utils.distBetweenPoints(newOrder[2], newOrder[3]));
	let h = Math.max(utils.distBetweenPoints(newOrder[1], newOrder[2]), utils.distBetweenPoints(newOrder[3], newOrder[0]));

	if (w == 0 || h == 0) {	return undefined;	}

	let from = newOrder;
	let to = [new cv.Point(0, 0),
			  new cv.Point(w - 1, 0),
			  new cv.Point(w - 1, h - 1),
			  new cv.Point(0, h - 1)];

	let m = cv.getPerspectiveTransform(from, to);

	return image.warpPerspective(m, new cv.Size(w, h));
};

/**
 * Write the image on disk
 * @param {cv.Mat} mat Opencv4NodeJS Mat represent image
 * @param {*} filename Filename to save
 */
exports.write = function (mat, filename) {
	cv.imwrite(filename, mat);
};


/**
 * Decode the image from Hololens
 * @param {*} stream
 * @returns {cv.Mat} mat Opencv4NodeJS Mat represent image
 */
exports.streamToMat = function (stream) {
	return cv.imdecode(stream);
};

/**
 * Change coding of mat
 * @param {cv.Mat} mat Opencv4NodeJS Mat represent image
 * @returns {*} buffer
 */
exports.matToBase64 = function (mat) {
	let buf = exports.matToStream(mat);
	return buf.toString('base64').replace(/-/g, "");
}

/**
 * Code the image to Hololens
 * @param {cv.Mat} mat Opencv4NodeJS Mat represent image
 * @returns {*} stream
 */
exports.matToStream = function (mat) {
	return Buffer.from(cv.imencode('.jpeg', mat), 'base64');
};
