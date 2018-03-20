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
var ObjectId = mongoose.Schema.Types.ObjectId;


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
  models.Link.find({ objects : firstDocumentId}).exec(function (err1, links1) {
    let L1 = undefined;
    if (links1 && links1.length > 0) {
      L1 = links1[0];
    }

    models.Link.find({ objects : secondDocumentId}).exec(function (err2, links2) {
      let L2 = undefined;
      if (links2 && links2.length > 0) {
        L2 = links2[0];
      }

      let updateCallback = function (err, link) {
        if (err) {
          if (errorCallback) errorCallback(err);
        } else {
          models.Link.findOne({ _id : link._id}).exec(function (error, updated) {
            if (updated){
              if (successCallback) successCallback(updated);
            } else {
              if (errorCallback) errorCallback(error);
            }
          });
        }
      }

      if (L1 && L2) {
        let objects = L1.objects.concat(L2.objects);

        models.Link.findByIdAndUpdate(L1._id, {objects: objects}).exec(updateCallback);

        models.Link.findByIdAndRemove (L2._id).exec();
      }
      else if (L1 || L2)
      {
        let objects = L1 ?
          L1.objects.concat(secondDocumentId) :
          L2.objects.concat(firstDocumentId);

        models.Link.findByIdAndUpdate((L1 ? L1._id : L2._id), {objects: objects}).exec(updateCallback);
      }
      else
      {
        let objects = [firstDocumentId, secondDocumentId];

        var link = new models.Link ({
          objects: objects
        });

        saveModel(link, successCallback, errorCallback);
      }
    });
  });
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

  if (first == second && callback) {
    callback(true);
    return;
  }

  models.Link.find({ objects: {$all: [first, second]}}).exec(function (err, links) {
    if (callback){
      if (links && links.length > 0) {
        callback(true);
      } else {
        callback(false);
      }
    }
  });
}

function getLink (documentId, callback) {
  let id = String(documentId);

  if (id) {
    models.Link.find({ objects: id}).exec( function (err, links) {
      if (callback && links) {
          callback(links.length > 0 ? links[0] : undefined);
      }
    });
  }
}

function deleteLink (id, successCallback, errorCallback) {
  models.Link.find({ objects : id}).exec(function (err, links) {
    //console.log(err);
    if (links && links.length > 0) {
      let l = links[0];

      let index = l.objects.findIndex(x => String(x) == String(id));
      if (index != -1) {
        var objects = l.objects.splice(index, 1);
        if (l.objects.length > 1) {
          models.Link.findByIdAndUpdate(l._id, {objects: objects}).exec(function (err, link) {
            if (err) {
              if (errorCallback) errorCallback(err);
            } else {
              models.Link.findOne({ _id : link._id}).exec(function (error, updated) {
                if (updated){
                  if (successCallback) successCallback(updated);
                } else {
                  if (errorCallback) errorCallback(error);
                }
              });
            }
          });
        } else {
          models.Link.findByIdAndRemove (l._id).exec();
        }
      } else {
        if (errorCallback) errorCallback();
      }
    } else {
      if (errorCallback) errorCallback(err);
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
exports.getLink = getLink;
exports.getDocumentCount = documentCount;
exports.dropEverything = dropEverything;
exports.matchFeatures = matchFeatures;
