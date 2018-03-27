var mongoose = require('mongoose');

var authorSchema = mongoose.Schema({
  name: String
});

module.exports = function (connection) {
  return connection.model('Author', authorSchema);
};
