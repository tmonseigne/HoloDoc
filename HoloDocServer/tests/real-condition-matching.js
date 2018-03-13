const cv = require('opencv4nodejs');
const improc = require('../improc/improc.js');

let clean = cv.imread('./images/5.jpg');
let photo = cv.imread('./photo.jpg');

let dist = improc.distance(improc.extractFeatures(clean, 25), improc.extractFeatures(photo, 25), [2, 2, 2, 1, 1, 1]);

console.log('Distance : ' + (1 - dist / Math.sqrt(2)) * 100);
