const cv = require('opencv4nodejs');

var MAGIC_NUMBER1 = 0.5;
var MAGIC_NUMBER2 = 75;

///////////////////////////// ALREADY MOVED ON improc-utils ////////////////////////////////

function arrayToVector3(array) {
	return new cv.Vec(array[0], array[1], array[2]);
}

function getColorRange(color, range = MAGIC_NUMBER2) {
	if (range > 127) {	range = 127;	}
	let lower = [0, 0, 0];
	let higher = [0, 0, 0];

	for (let i = 0; i < 3; ++i) {
		lower[i] = color[i] - range;
		higher[i] = color[i] + range;
		if (lower[i] < 0) {
			higher[i] -= lower[i];
			lower[i] = 0;
		}
		if (higher[i] > 255) {
			lower[i] -= higher[i] - 255;
			higher[i] = 255;
		}
	}

	return [arrayToVector3(lower), arrayToVector3(higher)];
}

function getCentroid(points) {
	let centroid = new cv.Point(0, 0);

	points.forEach(function (point) {
		centroid = centroid.add(point);
	});

	return centroid.div(points.length);
}

function addIfNotIn(arr, value) {
	if (arr.find(x => x == value) == undefined) {
		arr.push(value);
	}

	return arr;
}

function distBetweenPoints(p1, p2) {
	return p2.sub(p1).norm();
}

function findfarestPointFrom(points, from) {
	let distMax = 0;
	let idMax = 0;

	points.forEach(function (point, index) {
		let dist = distBetweenPoints(point, from);
		if (dist > distMax) {
			distMax = dist;
			idMax = index;
		}
	});

	return idMax;
}

function getPerimeter(points) {
	let distances = getDistances(points);

	let peri = distances.reduce(function (accu, value) {
		return accu + value;
	});

	return peri;
}

function getDistances(points) {
	let distances = points.map(function (value, index) {
		return distBetweenPoints(value, points[(index + 1) % points.length]);
	});

	return distances;
}

function extractCorners(contour, minLength, maxLength) {
	let points = contour.getPoints();
	let centroid = getCentroid(points);

	let corners = [];
	// we grab the farest point from the centroid and the farest from the first one to get the diagonal
	corners = addIfNotIn(corners, findfarestPointFrom(points, centroid));
	corners = addIfNotIn(corners, findfarestPointFrom(points, points[corners[0]]));

	// Now we try to maximaxe the area
	let areaMax = [0, 0];
	let idMax = [0, 0];

	let A = points[corners[0]];
	let B = points[corners[1]];
	let AB = B.sub(A);
	let dAB = AB.norm();

	points.forEach(function (C, index) {
		let AC = C.sub(A);
		let BC = C.sub(B);
		let d = AB.x * AC.y - AB.y * AC.x;
		if (d != 0) {
			let side = d > 0 ? 0 : 1;
			let dAC = AC.norm();
			let dBC = BC.norm();
			let peri = (dAB + dAC + dBC) / 2;
			let area = peri * (peri - dAC) * (peri - dAB) * (peri - dBC);

			if (area > areaMax[side]) {
				areaMax[side] = area;
				idMax[side] = index;
			}
		}
	});
	addIfNotIn(corners, idMax[0]);
	addIfNotIn(corners, idMax[1]);

	if (corners.length != 4) {	return undefined;	}
	// We arrange in this order to have the points in counter clockwise order
	let result = [points[corners[0]], points[corners[2]], points[corners[1]], points[corners[3]]];
	let peri = getPerimeter(result);

	if (!(minLength < peri && peri < maxLength)) {	return undefined;	}
	return result;
}

function verifyShape(points, ratio = MAGIC_NUMBER1) {
	let ratioMin = 1 - ratio;
	let ratioMax = 1 + ratio;
	let distances = getDistances(points);
	let ratio1 = distances[0] / distances[2];
	let ratio2 = distances[1] / distances[3];

	return (ratioMin < ratio1 && ratio1 < ratioMax &&
			ratioMin < ratio2 && ratio2 < ratioMax);
}

function getEstimatedArea(points) {
	return distBetweenPoints(points[0], points[1]) * distBetweenPoints(points[1], points[2]);
}

function strangeCrossProduct(p1, p2) {
	return p1.x * p2.y - p1.y * p2.x;
}

function inQuad(quad, point) {
	let cross = [0, 0];

	for (let i = 0; i < 4; ++i) {
		let sign = strangeCrossProduct(quad[(i + 1) % 4].sub(quad[i]), quad[i].sub(point)) >= 0 ? 1 : 0;
		cross[sign]++;
	}

	return (cross[0] == 0 || cross[1] == 0);
}

function sortDocCorners(doc) {
	let min = doc[0].x + doc[0].y;
	let id = 0;

	doc.forEach(function (value, index) {
		let sum = value.x + value.y;

		if (sum < min) {
			min = sum;
			id = index;
		}
	});

	let result = [];
	result.push(doc[id]);

	for (let i = 1; i < 4; ++i) {
		let j = id - i;
		if (j < 0) { j += 4; }

		result.push(doc[j]);
	}

	return result;
}


exports.getCenter = function (image) {
	return new cv.Point(image.cols / 2, image.rows / 2);
}

///////////////////////////// STAY HERE ////////////////////////////////

/**
 * Detect all Documents of an image
 * @param {cv.Mat} image Opencv4NodeJS Mat
 * @param {Array.<Number>} backgroundColor Array of three Number
 * @returns {Array.<Array.<cv.Point>>} Array of Array of Opencv4NodeJS points represented all docs in image
 */
exports.detectDocuments = function (image, backgroundColor) {
	console.log(backgroundColor);
	[lower, higher] = getColorRange(backgroundColor, 25);

  image = image.cvtColor(cv.COLOR_BGR2GRAY);
  image = image.gaussianBlur(new cv.Size(3,3), 0);
  let thresholdResult = image.canny(50, 205);
  thresholdResult = thresholdResult.dilate(new cv.Mat(3,3, cv.CV_8U, 1));
	let width = image.cols;
	let height = image.rows;
	let minLength = 0.2 * (width + height);
	let maxLength = 1.4 * (width + height);

	cv.imwrite('./seuillage.jpg', thresholdResult);

	let contours = thresholdResult.findContours(cv.RETR_LIST, cv.CHAIN_APPROX_SIMPLE);

	let result = [];
	let i = 0;

	for (let c of contours) {

		let perimeter = c.arcLength(true);

		if (c.numPoints >= 4 && minLength <= perimeter && perimeter <= maxLength) {
			let corners = extractCorners(c, minLength, maxLength);
			if (corners != undefined) {
				if (verifyShape(corners)) {
					result.push(corners);
				}
			}
		}
		i++;
	}

	result = result.sort(getEstimatedArea);

	for (let i = 0; i < result.length; ++i) {
		let points = result[i];

		for (let j = i + 1; j < result.length; ++j) {
			if (inQuad(points, getCentroid(result[j]))) {
				result.splice(j, 1);
				j--;
			}
		}
	}

	return result;
};

/**
 * Crop the quad and correct the perspective
 * @param {cv.Mat} image Opencv4NodeJS Mat represent image
 * @param {Array.<cv.Point>} doc Array of four Opencv4NodeJS points represent the doc to extract
 * @returns {cv.Mat} The undeformed quad
 */
exports.undistordDoc = function (image, doc) {
	let newOrder = sortDocCorners(doc); // illuminatus es spiritus sancti

	let w = Math.max(distBetweenPoints(newOrder[0], newOrder[1]), distBetweenPoints(newOrder[2], newOrder[3]));
	let h = Math.max(distBetweenPoints(newOrder[1], newOrder[2]), distBetweenPoints(newOrder[3], newOrder[0]));

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

///////////////////////////// ALREADY MOVED TO improc-recognition ////////////////////////////////

const getHistAxis = (channel, bins) => ([
	{
		channel,
		bins,
		ranges: [0, 256]
	}
]);

function getNormalizedHistogram(image, channel, bins) {
	let nbPixels = image.cols * image.rows;
	return cv.calcHist(image, getHistAxis(channel, bins)).div(nbPixels).transpose().getDataAsArray()[0];
}

exports.featuresDistance = function (features1, features2, coefs = [1, 1, 1, 1, 1, 1]) {
	let dist = 0;
	let cummulativeCoef = 0;

	for (let i = 0; i < features1.length; ++i) {
		let lineDifSquaredSum = 0;
		for (let j = 0; j < features1[i].length; ++j) {
			let val = features1[i][j] - features2[i][j];
			lineDifSquaredSum += val * val;
		}

		dist += coefs[i] * Math.sqrt(lineDifSquaredSum);
		cummulativeCoef += coefs[i];
	}

	return dist / cummulativeCoef;
};

exports.featureDistanceNormalization = function (distance) {
	return distance / exports.maxFeaturesDistance;
}

exports.maxFeaturesDistance = Math.sqrt(2);

exports.getNearestdocFrom = function (docs, from) {
	let distMin = Number.MAX_SAFE_INTEGER;
	let idMin = 0;

	docs.forEach(function (doc, index) {
		let centroid = getCentroid(doc);

		let dist = distBetweenPoints(from, centroid);
		if (dist < distMin) {
			distMin = dist;
			idMin = index;
		}
	});

	return docs[idMin];
}

exports.extractFeatures = function (image, bins = 25) {
	let nbPixels = image.cols * image.rows;

	let hsv = image.cvtColor(cv.COLOR_BGR2HSV);

	let histogramR = getNormalizedHistogram(image, 0, bins);
	let histogramG = getNormalizedHistogram(image, 1, bins);
	let histogramB = getNormalizedHistogram(image, 2, bins);

	let histogramH = getNormalizedHistogram(hsv, 0, bins);
	let histogramS = getNormalizedHistogram(hsv, 1, bins);
	let histogramV = getNormalizedHistogram(hsv, 2, bins);

	let result = [];
	result.push(histogramH, histogramS, histogramV, histogramR, histogramG, histogramB);

	return result;
};

///////////////////////////// USELESS ////////////////////////////////

var hogDescriptor = new cv.HOGDescriptor(new cv.Size(16, 16), new cv.Size(16, 16), new cv.Size(16, 16), new cv.Size(8, 8), 9);

function extractHOG(image) {
	return hogDescriptor.compute(image);
};

var rotatedRectToPoints = function (rect) {
	let points = [];

	let c = rect.center;
	let s = rect.size;
	let angle = - rect.angle * Math.PI / 180;

	let p1 = new cv.Point(- s.width / 2, - s.height / 2);
	let p2 = new cv.Point(+ s.width / 2, - s.height / 2);
	let p3 = new cv.Point(+ s.width / 2, + s.height / 2);
	let p4 = new cv.Point(- s.width / 2, + s.height / 2);

	points.push(c.add(rotatePoint2(p1, angle)));
	points.push(c.add(rotatePoint2(p2, angle)));
	points.push(c.add(rotatePoint2(p3, angle)));
	points.push(c.add(rotatePoint2(p4, angle)));

	return points;
}

var rotatePoint2 = function (point2, angle) {
	let cosTheta = Math.cos(angle);
	let sinTheta = Math.sin(angle);

	return new cv.Point(point2.x * cosTheta + point2.y * sinTheta,
		-point2.x * sinTheta + point2.y * cosTheta);

}
