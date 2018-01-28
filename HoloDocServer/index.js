var express = require('express');
var app = express();

var mainRoutes = require('./routes');
var docRoutes = require('./document_routes');

app.use('/', mainRoutes);
app.use('/document', docRoutes);

var server = app.listen(8080, function() {
  let host = server.address().address;
  let port = server.address().port;
  console.log("HoloDocServer running on http://%s:%s", host, port);
});
