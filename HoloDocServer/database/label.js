var mongoose = require('mongoose');

var labelSchema = mongoose.Schema({
  name: String
});


module.exports = function (connection) {
  return connection.model('Label', labelSchema);
};
