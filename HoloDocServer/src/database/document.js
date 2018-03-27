var mongoose = require('mongoose');

var documentSchema = mongoose.Schema({
  name: String,
  label: String,
  desc: String,
  author: String,
  date: Date,
  path: String,
  features: [[Number]],
  captured: { type: Date, default: Date.Now }
});

module.exports = function (connection) {
  return connection.model('Document', documentSchema);
};
