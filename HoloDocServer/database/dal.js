/**
 * This file define the data access layer.
 * This is here that we found all the function to access to the database.
 * Please only use these methods and not modify the database by yourself.
 **/

var mongoose = require('mongoose');

var models = require("./models.js");
var events = require('events');
var improc = require('../improc/improc.js');

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
        if (errorCallback)
        {
          console.log(err);
          errorCallback(err);
        }
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
  return models.db.connection.collections.documents.count();
}

function updateDocument (id, modifications, callback) {
  models.Document.findByIdAndUpdate(mongoose.Types.ObjectId(id), modifications).exec(function (err, document) {
    if (err) {
      errorEventEmiter.raiseError(err);
      return;
    }

    getDocuments({_id: document._id}, function(err, docs){
      if (callback && docs.length > 0) {
        callback(docs[0]);
      }
    });
  });
}

function getDocuments (query, callback) {
  models.Document.find(query).exec(callback);
}

function matchFeatures (features, callback) {
  if (features) {
    models.Document.find({}, function (err, documents) {
      let minDist = improc.maxFeaturesDistance;
      let minDoc;
      documents.forEach(function (doc, index) {
        let dist = improc.featuresDistance(doc.features, features);

        if (dist < minDist) {
          minDist = dist;
          minDoc = doc;
        }
      });

      // We compute the match percentage according to the distance
      let match = (1 - improc.featureDistanceNormalization(minDist)) * 100;

      if (match > 80) {
        minDoc.match = match;
        callback(minDoc);
      } else {
        callback(undefined);
      }
    });
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

  result = links.filter(function (link) {
    return link.from == from;
  });

  return result;
}

/**
 * This function will try to find a path between the two given documents.
 * return true if a path exists, false else.
 **/
function areConnected (firstDocumentId, secondDocumentId, callback) {
  let first = String(firstDocumentId);
  let second = String(secondDocumentId);

  models.Link.find({}).exec(function (err, links) {
      var marked = {};
      var queue = new Queue();

      queue.enqueue(first);

      marked[first] = true;

      while (!queue.isEmpty()) {
        let current = queue.dequeue();

        if (current == second && callback) {
           callback(true);
           return;
        }

        let neighbors = getNeighboors(links, current);

        neighbors.forEach(function (neighborLink) {
        	let to = String(neighborLink.to);
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

function deleteLink (from, to, successCallback, errorCallback) {
  let idForward, idBackward;

  models.Link.find({from:from, to:to}).exec(function (fferr, flinks) {
    idForward = flinks.length == 1 ? flinks[0]._id : undefined;
    models.Link.find({from:to, to:from}).exec(function (fberr, blinks) {
        idBackward = blinks.length == 1 ? blinks[0]._id : undefined;

        if (idForward && idBackward) {
          models.Link.findByIdAndRemove(idForward, function (ferr, flink) {
            models.Link.findByIdAndRemove(idBackward, function (berr, blink) {
              successCallback();
            });
          });
        } else {
          errorCallback();
        }
    });
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
exports.getDocumentCount = documentCount;
exports.dropEverything = dropEverything;
exports.matchFeatures = matchFeatures;
