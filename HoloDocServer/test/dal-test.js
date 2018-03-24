var assert = require('assert');

var globals = require ('../utils/globals.js');
globals.dbUri = 'mongodb://localhost/test';

var dal = require ('../database/dal.js');
var improc = require ('../improc/improc');
var cv = require('opencv4nodejs');

describe('Testing database access layer - ', function() {
  this.timeout(5000);
  let d1, d2, d3, d4, l1, l2;

  beforeEach(function(done) {
    dal.createDocument('one', 'article', '', 'arnaud', Date.Now, 'one.png', improc.extractFeatures(cv.imread('./test/res/2.jpg')),
      function(doc1) {
        d1 = doc1;
        dal.createDocument('two', 'picture', '', 'arnaud', Date.Now, 'two.png', improc.extractFeatures(cv.imread('./test/res/3.png')),
          function(doc2) {
            d2 = doc2;
            dal.createDocument('three', 'article', '', 'arnaud', Date.Now, 'three.png', improc.extractFeatures(cv.imread('./test/res/4.png')),
              function(doc3) {
                d3 = doc3;
                dal.createDocument('four', 'album', '', 'arnaud', Date.Now, 'four.png', improc.extractFeatures(cv.imread('./test/res/5.jpg')),
                  function(doc4) {
                    d4 = doc4;
                  dal.createLink(String(doc1._id), String(doc2._id),
                    function (link1){
                      l1 = link1;
                      dal.createLink(String(doc2._id), String(doc3._id),
                        function (link2){
                          l2 = link2;
                          done();
                        }, function (err){}
                      );

                    }, function (err){}
                  );
                }, function (err){}
              );
              }, function (err){}
            );
          }, function (err){}
        );
      }, function (err){}
    );
  });

  afterEach(function(done) {
    dal.dropEverything(function () {
      d1 = undefined;
      d2 = undefined;
      d3 = undefined;
      d4 = undefined;
      l1 = undefined;
      l2 = undefined;
      done();
    });
  });

  describe('Testing the initialisation', function () {
    it('Adding documents and links', function (done) {
      assert (d1 != undefined);
      assert (d2 != undefined);
      assert (d3 != undefined);
      assert (d4 != undefined);
      assert (l1 != undefined);
      assert (l2 != undefined);
      done();
    });

    it('Get Document function - naive', function (done) {
      dal.getDocuments({name: 'one'}, function (err, docs) {
        assert (docs.length == 1);
        let doc = docs[0];
        assert(String(doc._id) == String(d1._id));
        done();
      });

    });

    it('Get Document function - search something that does not exist', function (done) {
      dal.getDocuments({name: 'five'}, function (err, docs) {
        assert (docs.length == 0);
        done();
      });

    });

    it('Get Document function - multiple', function (done) {
      dal.getDocuments({label: 'article'}, function (err, docs) {
        assert (docs.length == 2);
        let doc1 = docs[0];
        let doc2 = docs[1];
        assert(String(doc1._id) == String(d1._id));
        assert(String(doc2._id) == String(d3._id));
        done();
      });
    });

    it('Get Document function - get all', function (done) {
      dal.getDocuments({}, function (err, docs) {
        assert (docs.length == 4);
        let doc1 = docs[0];
        let doc2 = docs[1];
        let doc3 = docs[2];
        let doc4 = docs[3];
        assert(String(doc1._id) == String(d1._id));
        assert(String(doc2._id) == String(d2._id));
        assert(String(doc4._id) == String(d4._id));
        done();
      });
    });

    it('Get Document Count function', function (done) {
      dal.getDocumentCount().then(function(count) {
        assert(count == 4);
        done();
      })
    });

    it('Get Link function - valid id with link', function (done) {
      dal.getLink(String(d1._id), function(link) {
        assert(link);
        assert(link.objects.length == 3);
        done();
      })
    });

    it('Get Link function - valid id without link', function (done) {
      dal.getLink(String(d4._id), function(link) {
        assert(!link);
        done();
      })
    });

    it('Get Link function - with no id', function (done) {
      dal.getLink(undefined, function(link) {
        assert(!link);
        done();
      })
    });
  });

  describe('Testing the document update function', function () {
    it('Updating an existing document', function (done) {
      dal.getDocuments({name: 'one'}, function (err, docs) {
        dal.updateDocument(docs[0]._id, {label: 'picture'}, function (doc) {

            assert(String(doc._id) == String(d1._id));
            assert(String(doc.label) != String(d1.label));
            assert(String(doc.label) == 'picture');
            done();
        });
      });
    });
  });

  describe('Testing the areConnected function', function () {
    it('Between two direct neighboors', function (done) {
      dal.areConnected(String(d1._id), String(d2._id), function (connected) {
        assert(connected);
        done();
      });
    });

    it('Between indirect neighboors', function (done) {
      dal.areConnected(String(d1._id), String(d3._id), function (connected) {
        assert(connected);
        done();
      });
    });

    it('Between two non connected document', function (done) {
      dal.areConnected(String(d1._id), String(d4._id), function (connected) {
        assert(!connected);
        done();
      });
    });
  });

  describe('Testing the invalid document creation', function () {
    it ('creating document with invalid parameters', function (done) {
      dal.createDocument([42, 43], -1, false, {x: 4, y:6}, 42, 42, [],
        function(doc) {

        },
        function (err) {
          done();
        });
    });
  });

  describe('Testing the features matching', function () {
    it ('In DB features', function (done) {
      dal.matchFeatures(improc.extractFeatures(cv.imread('./test/res/3.png')), function (doc) {
        assert(doc);
        assert(String(doc._id) == String(d2._id));
        done();
      });
    });

    it ('Not in DB features', function (done) {
      dal.matchFeatures(improc.extractFeatures(cv.imread('./test/res/6.jpg')), function (doc) {
        assert(!doc);
        done();
      });
    });

    it ('Invalid features - undefined', function (done) {
      dal.matchFeatures(undefined, function (doc) {
        assert(!doc);
        done();
      });
    });

    it ('Invalid features - wrong format', function (done) {
      dal.matchFeatures([[1,3,4], [4, 5, 6]], function (doc) {
        assert(!doc);
        done();
      });
    });
  });

  describe('Testing the link suppression', function () {
    it('Existing link', function (done) {
      dal.deleteLink(String(d1._id), function () {
        done();
      }, function () {});
    });

    it('Non Existing link', function (done) {
      dal.deleteLink(String(d4._id), function () {}, function () {
        done();
      });
    });

    it('without id', function (done) {
      dal.deleteLink('', function () {}, function () {
        done();
      });
    });
  });
});
