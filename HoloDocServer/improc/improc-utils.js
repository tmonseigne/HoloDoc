const cv = require('opencv4nodejs');

var MAGIC_NUMBER1 = 0.5;
var MAGIC_NUMBER2 = 25;

/**
 * Change an array of three numbers into one Vec3 for Opencv4NodeJS
 * @param {Array.<Number>} array array of three numbers
 * @returns {cv.Vec}  Opencv4NodeJS Vec3
 */
function arrayToVector3(array) {
	return new cv.Vec(array[0], array[1], array[2]);
}

/**
 * Get a two-range color interval on each channel with two Vec3 for Opencv4NodeJS
 * @param {Array.<Number>} color Original color  
 * @param {Number} range Half Range 
 * @returns {Array.<cv.Vec>}  Array of two Opencv4NodeJS Vec3, the lower and higher color 
 */
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

/**
 * Get the average points of an array of Opencv4NodeJS points 
 * @param {Array.<cv.Point>} points Array of Opencv4NodeJS points 
 * @returns {cv.Point} Centroid of the array
 */
function getCentroid(points) {
	let centroid = new cv.Point(0, 0);

	points.forEach(function (point) {
		centroid = centroid.add(point);
	});

	return centroid.div(points.length);
}

/**
 * Add an object if not find on the array to avoid duplication.
 * @param {Array.<Object>} arr array to edit
 * @param {Object} value value to push on the array
 * @return {Array.<Object>} the edited array
 */
function addIfNotIn(arr, value) {
	if (arr.find(x => x == value) == undefined) {
		arr.push(value);
	}

	return arr;
}

/**
 * Get the Euclidian Distance Between two points
 * @param {cv.Point} p1 Opencv4NodeJS point
 * @param {cv.Point} p2 Opencv4NodeJS point
 * @returns {Number} Euclidian Distance between the two points
 */
function distBetweenPoints(p1, p2) {
	return p2.sub(p1).norm();
}

/**
 * Find the farest point from the array to the point
 * @param {Array.<cv.Point>} points Array of Opencv4NodeJS points 
 * @param {cv.Point} from Opencv4NodeJS Point to compare
 * @returns {Number} The index of the farest point
 */
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

/**
 * Get the sum of the distance of an array of Opencv4NodeJS points 
 * (if the array is a contour it's the perimeter)
 * @param {Array.<cv.Point>} points Array of Opencv4NodeJS points
 * @returns {Number} Perimeter of the contour
 */
function getPerimeter(points) {
	let distances = getDistances(points);

	let peri = distances.reduce(function (accu, value) {
		return accu + value;
	});

	return peri;
}

/**
 * Get the distance between each Opencv4NodeJS point of an array 
 * (if the array is a closed polygone it's all side of this)
 * @param {Array.<cv.Point>} points Array of Opencv4NodeJS points
 * @returns {Array.<Number>} Length of all sides of the polygone
 */
function getDistances(points) {
	let distances = points.map(function (value, index) {
		return distBetweenPoints(value, points[(index + 1) % points.length]);
	});

	return distances;
}

/**
 * Extracts four corners of the contour that maximizes the area
 * @param {cv.Contour} contour Opencv4NodeJS Contour Object
 * @param {Number} minLength Minimum Length of perimeter 
 * @param {Number} maxLength Maximum Length of perimeter
 * @return {Array.<cv.Point>} Four corners of the contour
 */
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

	//AB is the diagonal
	let A = points[corners[0]];
	let B = points[corners[1]];
	let AB = B.sub(A);
	let dAB = AB.norm();

	points.forEach(function (C, index) {
		let AC = C.sub(A);
		let BC = C.sub(B);
		let d = AB.x * AC.y - AB.y * AC.x;
		if (d != 0) {
			let side = d > 0 ? 0 : 1;	//We check if C is on left or right side of Diagonal
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
	//If we have no chance there is only 3 points (but only murphy can produce this exception)
	if (corners.length != 4) {	return undefined;	}
	// We arrange in this order to have the points in counter clockwise order
	let result = [points[corners[0]], points[corners[2]], points[corners[1]], points[corners[3]]];

	//In very particular situation we can't pass this condition but it's not essential to verify
	let peri = getPerimeter(result);
	if (!(minLength < peri && peri < maxLength)) {	return undefined;	}
	return result;
}

/**
 * Verify if the quadrilateral is like a parallelogram (with a ratio threshold)
 * @param {Array.<cv.Point>} points Array of four Opencv4NodeJS points
 * @param {Number} ratio Ratio Treshold
 * @returns {Boolean} True if ratios are good, false if not
 */
function verifyShape(points, ratio = MAGIC_NUMBER1) {
	let ratioMin = 1 - ratio;
	let ratioMax = 1 + ratio;
	let distances = getDistances(points);
	let ratio1 = distances[0] / distances[2];
	let ratio2 = distances[1] / distances[3];

	return (ratioMin < ratio1 && ratio1 < ratioMax &&
			ratioMin < ratio2 && ratio2 < ratioMax);
}

/**
 * Get a false array (just multiply the two first side it's correct for rectangle)
 * @param {Array.<cv.Point>} points Array of minimum three Opencv4NodeJS points
 * @returns {Number} Area of a rectangle (false area for other quad)
 */
function getEstimatedArea(points) {
	return distBetweenPoints(points[0], points[1]) * distBetweenPoints(points[1], points[2]);
}

/**
 * Get a strange Cross product to know the sens of the two point (considered as 2D Vector)
 * @param {cv.Point} p1 Opencv4NodeJS points
 * @param {cv.Point} p2 Opencv4NodeJS points
 * @returns {Number} >0 if p1 is in right side of p2 <0 if not
 */
function strangeCrossProduct(p1, p2) {
	return p1.x * p2.y - p1.y * p2.x;
}

/**
 * Check if the point is in the quad 
 * @param {Array.<cv.Point>} quad Array of four Opencv4NodeJS points
 * @param {cv.Point} point Opencv4NodeJS points
 * @returns {Boolean} True if all verification have the same sign, False if not
 */
function inQuad(quad, point) {
	let cross = [0, 0];

	for (let i = 0; i < 4; ++i) {
		let sign = strangeCrossProduct(quad[(i + 1) % 4].sub(quad[i]), quad[i].sub(point)) >= 0 ? 1 : 0;
		cross[sign]++;
	}

	return (cross[0] == 0 || cross[1] == 0);
}

/**
 * Sort four corners doc detection fro counter clockwise order to clockwise order and Top-Left first
 * @param {Array.<cv.Point>} doc Array of four Opencv4NodeJS points
 * @returns {Array.<cv.Point>} Array sorted
 */
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

/**
 * Get the coordinates of the center of the image
 * @param {cv.Mat} image Image
 * @returns {cv.Point} The coordinates of the center of the image 
 */
exports.getCenter = function (image) {
	return new cv.Point(image.cols / 2, image.rows / 2);
}

/**
 * Get the Array of Opencv4NodeJS points where the centroid is closest to the point passed in parameter
 * @param {Array.<Array.<cv.Point>>} docs Array of Array of Opencv4NodeJS points
 * @param {cv.Point} from Opencv4NodeJS points
 * @returns {Array.<cv.Point>} Array of Opencv4NodeJS points selected
 */
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
