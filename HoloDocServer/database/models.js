var db = require("./db.js");

var connection = db.connection;

var Document = require('./../database/document.js')(connection);
var Author = require('./../database/author.js')(connection);
var Link = require('./../database/link.js')(connection);
var Label = require('./../database/label.js')(connection);

exports.db = db;
exports.Document = Document;
exports.Author = Author;
exports.Link = Link;
exports.Label = Label;
