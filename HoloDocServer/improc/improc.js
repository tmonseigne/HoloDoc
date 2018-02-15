const cv = require('opencv4nodejs');

exports.detectDocuments = function () {

}

exports.write = function (mat, filename) {
  cv.imwrite(filename, mat);
}

exports.streamToMat = function (stream) {
  return cv.imdecode(stream);
}

exports.matToStream = function (mat) {

}
