var mongoose = require('mongoose');

var linkSchema = mongoose.Schema({
  from: mongoose.Schema.Types.ObjectId,
  to: mongoose.Schema.Types.ObjectId
});

module.exports = function (connection) {
  return connection.model('Link', linkSchema);
};
