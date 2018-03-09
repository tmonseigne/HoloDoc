var mongoose = require('mongoose');

var connection = mongoose.createConnection('mongodb://localhost/myapp');

var Document = require('./../database/document.js')(connection);
var Author = require('./../database/author.js')(connection);
var Link = require('./../database/link.js')(connection);
var Label = require('./../database/label.js')(connection);
