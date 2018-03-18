var mongoose = require('mongoose');

var linkSchema = mongoose.Schema({
  objects: [String],
});

module.exports = function (connection) {
  return connection.model('Link', linkSchema);
};
