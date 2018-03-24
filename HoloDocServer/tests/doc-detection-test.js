const cv = require('opencv4nodejs');
const improc = require('../improc/improc.js');
const utils = require('../improc/improc-utils.js');

let image = cv.imread('./D.jpg');

let bidule = improc.detectDocuments(image, [25, 25, 25]);

let colors = [new cv.Vec(255, 0, 0),
              new cv.Vec(0, 255, 0),
              new cv.Vec(0, 0, 255),
              new cv.Vec(255, 255, 0)]

bidule.forEach (function (points) {
  points.forEach (function (point, index) {
    image.drawCircle(point, 6, colors[index], -1);
  });
});

cv.imwrite('result.jpg', image);

let center = utils.getCenter(image);
let toExtract = utils.getNearestdocFrom(bidule, center);

let croped = improc.undistordDoc(image, toExtract);

if(croped) cv.imwrite('final.jpg', croped);
