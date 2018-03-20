const cv = require('opencv4nodejs');
const improc = require('../improc/improc.js');

let image = cv.imread('./bla2.jpg');

let bidule = improc.detectDocuments(image, [25, 25, 25]);

let colors = [new cv.Vec(255, 0, 0),
              new cv.Vec(0, 255, 0),
              new cv.Vec(0, 0, 255),
              new cv.Vec(255, 255, 0)]

bidule.forEach (function (points) {
  points.forEach (function (point, index) {
    image.drawCircle(point, 4, colors[index], -1);
  });
});

cv.imwrite('result.jpg', image);

let center = improc.getCenter(image);
let toExtract = improc.getNearestdocFrom(bidule, center);

let croped = improc.undistordDoc(image, toExtract);

cv.imwrite('final.jpg', croped);
