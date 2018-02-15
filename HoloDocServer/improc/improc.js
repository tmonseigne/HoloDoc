const cv = require('opencv4nodejs');

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
      console.log(c.numPoints);
    }
    i++;
  }

  return result;
}

exports.write = function (mat, filename) {
  cv.imwrite(filename, mat);
}

exports.streamToMat = function (stream) {
  return cv.imdecode(stream);
}

exports.matToStream = function (mat) {

}

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
