var assert = require('assert');

const cv = require('opencv4nodejs');
const utils = require('../improc/improc-utils.js');
const improc = require('../improc/improc.js');
const reco = require('../improc/improc-recognition.js');


describe('Testing Image Processing ', function() {
  this.timeout(5000);
  let im1, im2, im3;

  beforeEach(function(done) {
    im1 = cv.imread('./test/res/1.jpg');
    im2 = cv.imread('./test/res/2.jpg');
    im3 = undefined;
    done();
  });

  afterEach(function(done) {
      im1 = undefined;
      im2 = undefined;
      im3 = undefined;
      done();
    });

    describe('Testing Document Detection', function () {

      it('Document Detected on good image', function (done) {
        let Docs = improc.detectDocuments(im1);
        assert(Docs.length == 2);
        assert(Docs[0].length == 4 && Docs[1].length == 4);
        done();
      });

      it('Document Detected on false image', function (done) {
        let Docs = improc.detectDocuments(im2);
        assert(Docs.length == 0);
        done();
      });

      it('Document Detected on no image', function (done) {
        let Docs = improc.detectDocuments(im3);
        assert(Docs.length == 0);
        done();
      });

    });

    describe('Testing Document Extraction', function () {

      it('Document Extraction on good image', function (done) {
        let Docs = improc.detectDocuments(im1);
        let center = utils.getCenter(im1);
        let toExtract = utils.getNearestdocFrom(Docs, center);
        let croped = improc.undistordDoc(im1, toExtract);

        assert(croped);
        done();
      });

      it('Document Extraction on false image', function (done) {
        let Docs = improc.detectDocuments(im2);
        let center = utils.getCenter(im2);
        let toExtract = utils.getNearestdocFrom(Docs, center);
        let croped = improc.undistordDoc(im2, toExtract);

        assert(!croped);
        done();
      });

      it('Document Extraction on no image', function (done) {
        let Docs = improc.detectDocuments(im3);
        let center = utils.getCenter(im3);
        let toExtract = utils.getNearestdocFrom(Docs, center);
        let croped = improc.undistordDoc(im3, toExtract);

        assert(!croped);
        done();
      });

    });
});
