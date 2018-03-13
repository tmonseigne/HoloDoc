const cv = require('opencv4nodejs');
const improc = require('../improc/improc.js');
const csv = require('csv');
var fs = require('fs');

let nbFiles = 10;

let images = [];

for (let i = 1; i <= nbFiles; ++i) {
  images.push(cv.imread(`./images/${i}.jpg`));
}

let features = [];

for (let i = 0; i < nbFiles; ++i) {
  features.push(improc.extractFeatures(images[i], 25));
}

let distances = [];
for (let i = 0; i < nbFiles; ++i) {
  let row = [];
  for (let j = 0; j < nbFiles; ++j) {
    row.push((1 - (improc.distance(features[i],features[j], [2,2,2,1,1,1]) / Math.sqrt(2))) * 100);
  }
  distances.push(row);
}

var lineArray = [];
distances.forEach( function (row, index) {
  var line = row.join(';');
  lineArray.push(line);
});
var csvContent = lineArray.join('\n');


fs.writeFile("./distances.csv", csvContent, function(err) {
    if(err) {
        return console.log(err);
    }

    console.log("The file was saved!");
});
