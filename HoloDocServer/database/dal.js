/**
 * This file define the data access layer.
 * This is here that we found all the function to access to the database.
 * Please only use these methods and not modify the database by yourself.
 **/

var mongoose = require('mongoose');

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
function saveModel (model, successCallback, errorCallback){
  try {
    model.save ( function (err) {

      if (err) {
	errorEventEmiter.raiseError(err);
	return;
      }

      if (successCallback)
      {
	successCallback(model);
      }

      //console.log(model.constructor + " created !");
    });
  } catch (exception) {
    errorEventEmiter.raiseError(exception);
    if (errorCallback)
    {
      errorCallback(exception);
    }
  }
};

/**
 * Document access functions.
 * We only authorize creation, update and access operations.
 **/
function createDocument (name, label, desc, author, date, path, features, successCallback, errorCallback) {
  var document = new models.Document({
    name: name,
    label: label,
    desc: desc,
    author: author,
    date: date,
    path: path,
    features: features
  });

  saveModel(document, successCallback, errorCallback);
}

function documentCount () {
  return models.db.connection.Document.count();
}

function updateDocument (id, modifications, callback) {
  models.Document.findByIdAndUpdate(mongoose.Types.ObjectId(id), modifications).exec(function (err, document) {
    if (err) {
      errorEventEmiter.raiseError(err);
      return;
    }

    if (callback) {
      callback(document);
    }
  });
}

function getDocuments (query, callback) {
  models.Document.find(query).exec(callback);
}

function matchFeatures (features, callback) {
  if (callback) {
    callback (undefined);
  }
}

/**
 * Link access functions.
 * We only authorize creation, deletion and access operations.
 * We also propose a function to check if two doccuments are
 * in a same conex component.
 **/
function createLink(firstDocumentId, secondDocumentId, successCallback, errorCallback) {
  var forward = new models.Link({
    from: firstDocumentId,
    to: secondDocumentId
  });
  saveModel(forward);

  var backward = new models.Link({
    from: secondDocumentId,
    to: firstDocumentId
  });
  saveModel(backward, successCallback, errorCallback);
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
  models.Link.find({}).exec(function (err, links) {
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

function dropEverything (callback) {
  let count = 0;

  let fnc = function(err) {
    if (++count == 2 && callback) {
      callback();
    }
  };

  models.Link.remove({},fnc);
  models.Document.remove({},fnc);
}

/**
 * Exporting everything.
 **/
exports.createDocument = createDocument;
exports.updateDocument = updateDocument;
exports.getDocuments = getDocuments;
exports.areConnected = areConnected;
exports.createLink = createLink;
exports.deleteLink = deleteLink;

exports.dropEverything = dropEverything;
