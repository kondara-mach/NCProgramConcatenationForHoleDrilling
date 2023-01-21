﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.ParameterRewriter.Tests
{
    [TestClass()]
    public class DrillingParameterRewriterTests
    {
        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 2000, 150)]
        [DataRow(MaterialType.Iron, 1500, 100)]
        public void 正常系_センタードリルプログラムがドリルパラメータで書き換えられること(MaterialType material, int expectedSpin, int expectedFeed)
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create(material: material);
            IMainProgramParameterRewriter drillingParameterRewriter = new DrillingParameterRewriter();
            var actual = drillingParameterRewriter.RewriteByTool(param);

            // then
            decimal rewritedSpin = NCWordから値を取得する(actual, 'S');
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NCWordから値を取得する(actual, 'Z');
            decimal expectedCenterDrillDepth = param.DrillingPrameters
                .Select(x => x.CenterDrillDepth)
                .FirstOrDefault();
            Assert.AreEqual(expectedCenterDrillDepth, rewritedDepth, "Z値");

            var rewritedFeed = NCWordから値を取得する(actual, 'F');
            Assert.AreEqual(expectedFeed, rewritedFeed, "送り");
        }

        private static decimal NCWordから値を取得する(IEnumerable<NCProgramCode> expected, char address, int skip = 0)
        {
            return expected.Skip(skip).Select(x => x.NCBlocks)
                .SelectMany(x => x)
                .Where(x => x != null)
                .Select(x => x?.NCWords)
                .Where(x => x != null)
                .SelectMany(x => x!)
                .Where(y => y!.GetType() == typeof(NCWord))
                .Cast<NCWord>()
                .Where(z => z.Address.Value == address)
                .Select(z => z.ValueData.Number)
                .FirstOrDefault();
        }

        [TestMethod]
        public void 異常系_素材が未定義の場合例外を返すこと()
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create(material: MaterialType.Undefined);
            void target()
            {
                IMainProgramParameterRewriter drillingParameterRewriter = new DrillingParameterRewriter();
                _ = drillingParameterRewriter.RewriteByTool(param);
            }

            // then
            var ex = Assert.ThrowsException<ArgumentException>(target);
            Assert.AreEqual("素材が未定義です", ex.Message);
        }

        [TestMethod]
        public void 異常系_リストに一致するドリル径が無いとき例外を返すこと()
        {
            // given
            // when
            decimal diameter = 3m;
            var param = TestRewriteByToolRecordFactory.Create(targetToolDiameter: diameter);
            void target()
            {
                IMainProgramParameterRewriter drillingParameterRewriter = new DrillingParameterRewriter();
                _ = drillingParameterRewriter.RewriteByTool(param);
            }

            // then
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            Assert.AreEqual($"ドリル径 {diameter}のリストがありません",
                ex.Message);
        }

        [DataTestMethod]
        [DataRow(MaterialType.Aluminum, 10.5)]
        [DataRow(MaterialType.Iron, 12.4)]
        public void 正常系_下穴プログラムがドリルパラメータで書き換えられること(MaterialType material, double thickness)
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create();
            var drillingParameterRewriter = new DrillingParameterRewriter();
            var actual = drillingParameterRewriter.RewriteByTool(param);

            // then
            var rewritedSpin = NCWordから値を取得する(actual, 'S');
            var expectedSpin = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.SpinForAluminum)
                : ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.SpinForIron);
            Assert.AreEqual(expectedSpin, rewritedSpin, "下穴の回転数");

            decimal rewritedDepth = NCWordから値を取得する(actual, 'Z');
            decimal expectedDepth = ドリルパラメータから値を取得する(param.DrillingPrameters, x => -x.DrillTipLength - (decimal)thickness);
            Assert.AreEqual(expectedDepth, rewritedDepth, "下穴のZ");

            decimal rewritedCutDepth = NCWordから値を取得する(actual, 'Q');
            decimal expectedCutDepth = ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.CutDepth);
            Assert.AreEqual(expectedCutDepth, rewritedCutDepth, "下穴の切込");

            decimal rewritedFeed = NCWordから値を取得する(actual, 'F');
            decimal expectedFeed = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.FeedForAluminum)
                : ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.FeedForIron);
            Assert.AreEqual(expectedFeed, rewritedFeed, "下穴1の送り");
        }

        private static decimal ドリルパラメータから値を取得する(IEnumerable<DrillingProgramPrameter> drillingProgramPrameter, Func<DrillingProgramPrameter, decimal> select, int skip = 0)
        {
            return drillingProgramPrameter.Skip(skip)
                .Select(x => select(x))
                .FirstOrDefault();
        }

        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 1400)]
        [DataRow(MaterialType.Iron, 1100)]
        public void 正常系_面取りプログラムがドリルパラメータで書き換えられること(MaterialType material, int expectedSpin)
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create(material: material);
            IMainProgramParameterRewriter drillingParameterRewriter = new DrillingParameterRewriter();
            var actual = drillingParameterRewriter.RewriteByTool(param);

            // then
            decimal rewritedSpin = NCWordから値を取得する(actual, 'S');
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NCWordから値を取得する(actual, 'Z');
            decimal? expectedChamferingDepth = param.DrillingPrameters
                .Select(x => x.ChamferingDepth)
                .FirstOrDefault();
            Assert.AreEqual(expectedChamferingDepth, rewritedDepth, "面取り深さ");
        }
    }
}