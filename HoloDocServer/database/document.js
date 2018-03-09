var mongoose = require('mongoose');

var documentSchema = mongoose.Schema({
  name: String,
  label: mongoose.Schema.Types.ObjectId,
  desc: String,
  author: mongoose.Schema.Types.ObjectId,
  date: Date,
  path: String,
  features: [Number],
  captured: { type: Date, default: Date.Now }
});

module.exports = function (connection) {
  return connection.model('Document', documentSchema);
};
