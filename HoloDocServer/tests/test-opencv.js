const home = 'C:/Users/Licorne/Desktop/Arnaud/HoloDoc/HoloDocServer';

const cv = require('opencv4nodejs');
const improc = require(home + '/improc/improc');

// load image from file
let mat = cv.imread(home + '/tests/B.jpg');

let contours = improc.detectDocuments(mat, new cv.Vec(0,0,0), new cv.Vec(50,50,50));

console.log(contours);

for (let i = 0; i < contours.length; i += 4) {
  mat.drawPolylines ([contours.slice(i, i+4)] , true , new cv.Vec(0, 0, 255), 3);
}

// save image
cv.imwrite('result.jpg', mat);
