const cv = require('opencv4nodejs');

const HIST_BINS = 25;
exports.HIST_BINS = HIST_BINS;

const HIST_COEFS = [2, 2, 2, 1, 1, 1];
exports.HIST_COEFS = HIST_COEFS;

const MAX_FEATURES_DISTANCE = Math.sqrt(2);
exports.MAX_FEATURES_DISTANCE = MAX_FEATURES_DISTANCE;

/**
 * Hist Options for calcHist
 * @param {Number} channel Channel number in the image
 * @param {Number} bins Number of bins in the Histogramms
 */
const getHistAxis = (channel = 0, bins = HIST_BINS) => ([
	{
		channel,
		bins,
		ranges: [0, 256]
	}
]);

/**
 * Get Normalized Histogramm
 * @param {cv.Mat} image Image
 * @param {Number} channel Channel number in the image
 * @param {Number} bins Number of bins in the Histogramms
 * @returns {Array.<Number>} Histogramm of the channel with N bins
 *
 */
function getNormalizedHistogram(image, channel = 0, bins = HIST_BINS) {
	let nbPixels = image.cols * image.rows;
	return cv.calcHist(image, getHistAxis(channel, bins)).div(nbPixels).transpose().getDataAsArray()[0];
}

/**
 * Compare two set of features with weight
 * @param {Array.<Array.<Number>>} features1 Features One
 * @param {Array.<Array.<Number>>} features2 Features Two
 * @param {Array.<Number>} coefs Weight (Array of Six Numbers) => (H,S,V,B,G,R)
 * @returns {Number} Weighted Mean Distance of Six Features
 */
exports.featuresDistance = function (features1, features2, coefs = HIST_COEFS) {
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

/**
 * Normalize Distance by the maximum distance possible
 * @param {Number} distance Distance to normalized
 * @returns {Number} Distance Normalized
 */
exports.featureDistanceNormalization = function (distance) {
	return distance / MAX_FEATURES_DISTANCE;
}

/**
 * Extract all features
 * @param {cv.Mat} image Image in BGR
 * @param {Number} bins Number of bins in the Histogramms
 * @returns {Array.<Array.<Number>>} Features represented by six rows and N columns
 */
exports.extractFeatures = function (image, bins = HIST_BINS) {
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
