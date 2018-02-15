const cv = require('../improc/opencv4nodejs');

// load image from file
const mat = cv.imread('./img.jpg');

// show image
cv.imshow('a window name', mat);
cv.waitKey();

// save image
cv.imwrite('img2.png', mat);
