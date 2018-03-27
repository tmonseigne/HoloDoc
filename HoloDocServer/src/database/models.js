var db = require("./db.js");

var connection = db.connection;

var Document = require('./../database/document.js')(connection);
var Link = require('./../database/link.js')(connection);

exports.db = db;
exports.Document = Document;
exports.Link = Link;
