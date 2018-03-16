var express = require('express');
var app = express();

var bodyParser = require('body-parser');

var options = {
    inflate: true,
    limit: '10000kb',
    type: 'application/octet-stream'
};

app.use(bodyParser.raw(options));

var mainRoutes = require('./routes/routes');
var docRoutes = require('./routes/document-routes');
var linkRoutes = require('./routes/link-routes');

app.use('/', mainRoutes);
app.use('/document', docRoutes);
app.use('/link', linkRoutes);

var server = app.listen(8080, function() {
  let host = server.address().address;
  let port = server.address().port;
  console.log("HoloDocServer running on http://%s:%s", host, port);
});
