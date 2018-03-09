/**
  * This file define the data access layer.
  * This is here that we found all the function to access to the database.
  * Please only use these methods and not modify the database by yourself.
  **/

var models = require("./models.js");
var events = require('events');

var Queue = require('../utils/queue.js');

var errorEventEmiter = new events.EventEmitter;

errorEventEmiter.raiseError = function (err){
	var data = {
		error: err
	};
};


/**
  * This function save the given model and test for any exception.
  * If an exception occurs, then an error event is raised.
  **/
function saveModel (model){
	try {
		model.save ( function (err) {
		
			if (err) {
				errorEventEmiter.raiseError(err);
				return;
			}
			
			console.log(model.constructor + " created !");
		});
	} catch (exception) {
		errorEventEmiter.raiseError(exception);
	}
	
};

/**
  * Document access functions.
  * We only authorize creation, update and access operations.
  **/
function createDocument (name, labelId, desc, authorId, date, path, features, captured) {
  var document = new models.Document({
    name: name,
    label: labelId,
    desc: desc,
    author: authorId,
    date: date,
    path: path,
    features: features
  });

  saveModel(document);
}

function updateDocument (id, modifications) {
  models.Document.findByIdAndUpdate(id, modifications, function (err, document) {
    if (err) {
      errorEventEmiter.raiseError(err);
      return;
    }
  });
}

function getDocuments (query, callback) {
  models.Document.find(query, callback);
}

/**
  * Label access functions.
  * We only authorize creation and access operations.
  **/
function createLabel (name) {
  var label = new models.Label({name: name});
  
  saveModel(label);
}

function getLabels (query, callback) {
  models.Label.find(query, callback);
}

/**
  * Author access functions.
  * We only authorize creation and access operations.
  **/
function createAuthor (name) {
  var author = new models.Author({name});

  saveModel(author);
}

function getAuthors (query, callback) {
  models.Author.find(query, callback);
}

/**
  * Link access functions.
  * We only authorize creation, deletion and access operations.
  * We also propose a function to check if two doccuments are 
  * in a same conex component.
  **/
function createLink(firstDocumentId, secondDocumentId) {
  var forward = new models.Link({
    from: firstDocumentId,
    to: secondDocumentId
  });
  saveModel(forward);

  var backward = new models.Link({
    from: secondDocumentId,
    to: firstDocumentId
  });
  saveModel(backward);
}

function getNeighboors (links, from) {
  let result = [];

  links.filter(function (link) {
    return link.from == from;
  });

  return result;
}

/**
  * This function will try to find a path between the two given documents.
  * return true if a path exists, false else.
  **/
function areConnected (firstDocumentId, secondDocumentId, callback) {
  models.Link.find({}, function (err, links) {
    var marked = {};
    var queue = new Queue();
    
    queue.enqueue(firstDocumentId);

    marked[firstDocumentId] = true;

    while (!queue.isEmpty) {
      let current = queue.dequeue();

      if (current == secondDocumentId && callback) {
	callback(true);
	return;
      }

      let neighbors = getNeighboors(links, current);

      neighbors.forEach(function (neighborLink) {
	let to = neighborLink.to;

	if (!marked[to]) {
	  queue.enqueue(to);
	  marked[to] = true;
	}
      });
      
    }

    if (callback) {
      callback(false);
    }
  });
}

function deleteLink (linkId) {
  models.Link.findByIdAndRemove(linkId, function(err, link){
    if (err) {
      errorEventEmiter.raiseError(err);
      return;
    }
  });
}

/**
  * Exporting everything.
  **/
exports.createDocument = createDocument;
exports.updateDocument = updateDocument;
exports.getDocuments = getDocuments;

exports.createLink = createAuthor;
exports.getLinks = getAuthors;

exports.createLink = createLabel;
exports.getLinks = getLabels;

exports.createLink = createLink;
exports.getLinks = getLinks;
