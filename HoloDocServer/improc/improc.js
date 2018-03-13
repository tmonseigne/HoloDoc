const cv = require('opencv4nodejs');
const mathjs = require('mathjs');

var hogDescriptor = new cv.HOGDescriptor(new cv.Size(16, 16), new cv.Size(16,16), new cv.Size(16,16), new cv.Size(8, 8), 9);

exports.detectDocuments = function (image, lower, upper) {
  const thresholdResult = image.inRange(lower, upper);

  let width = image.cols;
  let height = image.rows;

  let contours = thresholdResult.findContours(1, 1);

  let minLength = 0.2 * (width + height);
  let maxLength = 1.4 * (width + height);


  let result = [];
  let i = 0;
  for (let c of contours) {
    let perimeter = c.arcLength(true);

    if (c.numPoints >= 4 && minLength <= perimeter && perimeter <= maxLength) {
      result = result.concat(rotatedRectToPoints(c.minAreaRect()));
      //console.log(c.numPoints);
    }
    i++;
  }

  return result;
};

exports.write = function (mat, filename) {
  cv.imwrite(filename, mat);
};

exports.streamToMat = function (stream) {
  return cv.imdecode(stream);
};

exports.matToStream = function (mat) {

};

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

exports.distance = function (features1, features2, coefs = [1,1,1,1,1,1]) {
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

exports.extractFeatures = function (image, bins) {
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

function extractHOG (image) {
  return hogDescriptor.compute(image);
};

var rotatedRectToPoints = function (rect) {
  let points = [];

  let c = rect.center;
  let s = rect.size;
  let angle = - rect.angle * Math.PI / 180;

  let p1 = new cv.Point (- s.width/2, - s.height/2);
  let p2 = new cv.Point (+ s.width/2, - s.height/2);
  let p3 = new cv.Point (+ s.width/2, + s.height/2);
  let p4 = new cv.Point (- s.width/2, + s.height/2);

  points.push(c.add(rotatePoint2(p1, angle)));
  points.push(c.add(rotatePoint2(p2, angle)));
  points.push(c.add(rotatePoint2(p3, angle)));
  points.push(c.add(rotatePoint2(p4, angle)));

  return points;
}

var rotatePoint2 = function(point2, angle){
  let cosTheta = Math.cos(angle);
  let sinTheta = Math.sin(angle);

  return new cv.Point(point2.x * cosTheta + point2.y * sinTheta,
                      -point2.x * sinTheta + point2.y * cosTheta);

}
