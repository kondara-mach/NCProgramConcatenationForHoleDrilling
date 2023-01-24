﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.ParameterRewriter.Tests
{
    [TestClass()]
    public class SkillReamingParameterRewriterTests
    {
        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 2000, 150)]
        [DataRow(MaterialType.Iron, 1500, 100)]
        public void 正常系_センタードリルプログラムがリーマパラメータで書き換えられること(MaterialType material, int expectedSpin, int expectedFeed)
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create(material: material);
            IMainProgramParameterRewriter skillReamingParameterRewriter = new SkillReamingParameterRewriter();
            var actual = skillReamingParameterRewriter.RewriteByTool(param);

            // then
            decimal rewritedSpin = NCWordから値を取得する(actual, 'S');
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NCWordから値を取得する(actual, 'Z');
            decimal expectedCenterDrillDepth = param.SkillReamerParameters
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
                IMainProgramParameterRewriter skillReamingParameterRewriter = new SkillReamingParameterRewriter();
                _ = skillReamingParameterRewriter.RewriteByTool(param);
            }

            // then
            var ex = Assert.ThrowsException<ArgumentException>(target);
            Assert.AreEqual("素材が未定義です", ex.Message);
        }

        [TestMethod]
        public void 異常系_リストに一致するリーマ径が無いとき例外を返すこと()
        {
            // given
            // when
            decimal diameter = 3m;
            var param = TestRewriteByToolRecordFactory.Create(targetToolDiameter: diameter);

            void target()
            {
                IMainProgramParameterRewriter skillReamingParameterRewriter = new SkillReamingParameterRewriter();
                _ = skillReamingParameterRewriter.RewriteByTool(param);
            }

            // then
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            Assert.AreEqual($"リーマ径 {diameter}のリストがありません",
                ex.Message);
        }

        [DataTestMethod]
        [DataRow(MaterialType.Aluminum, 10.5)]
        [DataRow(MaterialType.Iron, 12.4)]
        public void 正常系_下穴プログラムがリーマパラメータで書き換えられること(MaterialType material, double thickness)
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create(material: material, thickness: (decimal)thickness);
            var skillReamingParameterRewriter = new SkillReamingParameterRewriter();
            var actual = skillReamingParameterRewriter.RewriteByTool(param);

            // then
            var rewritedSpin = NCWordから値を取得する(actual, 'S');
            var spin = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.SpinForAluminum)
                : ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.SpinForIron);
            Assert.AreEqual(spin, rewritedSpin, "下穴1の回転数");

            rewritedSpin = NCWordから値を取得する(actual, 'S', 1);
            spin = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.SpinForAluminum, 1)
                : ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.SpinForIron, 1);
            Assert.AreEqual(spin, rewritedSpin, "下穴2の回転数");

            decimal rewritedDepth = NCWordから値を取得する(actual, 'Z');
            decimal depth = ドリルパラメータから値を取得する(param.DrillingPrameters, x => -x.DrillTipLength - (decimal)thickness);
            Assert.AreEqual(depth, rewritedDepth, "下穴1のZ");

            rewritedDepth = NCWordから値を取得する(actual, 'Z', 1);
            depth = ドリルパラメータから値を取得する(param.DrillingPrameters, x => -x.DrillTipLength - (decimal)thickness, 1);
            Assert.AreEqual(depth, rewritedDepth, "下穴2のZ");

            decimal rewritedCutDepth = NCWordから値を取得する(actual, 'Q');
            decimal cutDepth = ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.CutDepth);
            Assert.AreEqual(cutDepth, rewritedCutDepth, "下穴1の切込");

            cutDepth = cutDepth = ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.CutDepth, 1);
            rewritedCutDepth = NCWordから値を取得する(actual, 'Q', 1);
            Assert.AreEqual(cutDepth, rewritedCutDepth, "下穴2の切込");

            decimal rewritedFeed = NCWordから値を取得する(actual, 'F');
            decimal feed = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.FeedForAluminum)
                : ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.FeedForIron);
            Assert.AreEqual(feed, rewritedFeed, "下穴1の送り");

            rewritedFeed = NCWordから値を取得する(actual, 'F', 1);
            feed = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.FeedForAluminum, 1)
                : ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.FeedForIron, 1);
            Assert.AreEqual(feed, rewritedFeed, "下穴2の送り");
        }

        private static decimal ドリルパラメータから値を取得する(IEnumerable<DrillingProgramPrameter> drillingProgramPrameter, Func<DrillingProgramPrameter, decimal> select, int skip = 0)
        {
            return drillingProgramPrameter.Skip(skip)
                .Select(x => select(x))
                .FirstOrDefault();
        }

        [TestMethod]
        public void 異常系_下穴1回目に該当するドリル径が無いとき例外を返すこと()
        {
            // given
            // when
            decimal reamerDiameter = 5.5m;
            var param = TestRewriteByToolRecordFactory.Create(
                targetToolDiameter: reamerDiameter,
                crystalReamerParameters: new List<ReamingProgramPrameter>
                {
                    TestReamingProgramPrameterFactory.Create(DiameterKey: reamerDiameter.ToString(), PreparedHoleDiameter: 3),
                },
                drillingPrameters: new List<DrillingProgramPrameter>
                {
                    TestDrillingProgramPrameterFactory.Create(DiameterKey: "20"),
                    TestDrillingProgramPrameterFactory.Create(DiameterKey: "22"),
                });


            void target()
            {
                IMainProgramParameterRewriter skillReamingParameterRewriter = new SkillReamingParameterRewriter();
                _ = skillReamingParameterRewriter.RewriteByTool(param);
            }

            // then
            var fastDrill = param.SkillReamerParameters
                .Select(x => x.PreparedHoleDiameter)
                .FirstOrDefault();
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            Assert.AreEqual($"穴径に該当するリストがありません 穴径: {fastDrill}",
                ex.Message);
        }

        [TestMethod]
        public void 異常系_下穴2回目に該当するドリル径が無いとき例外を返すこと()
        {
            // given
            // when
            decimal reamerDiameter = 5.5m;
            var param = TestRewriteByToolRecordFactory.Create(
                targetToolDiameter: reamerDiameter,
                crystalReamerParameters: new List<ReamingProgramPrameter>
                {
                    TestReamingProgramPrameterFactory.Create(
                        DiameterKey: reamerDiameter.ToString(),
                        PreparedHoleDiameter: 20m,
                        SecondPreparedHoleDiameter: 3m),
                },
                drillingPrameters: new List<DrillingProgramPrameter>
                {
                    TestDrillingProgramPrameterFactory.Create(DiameterKey: "20"),
                    TestDrillingProgramPrameterFactory.Create(DiameterKey: "22"),
                });

            void target()
            {
                IMainProgramParameterRewriter skillReamingParameterRewriter = new SkillReamingParameterRewriter();
                _ = skillReamingParameterRewriter.RewriteByTool(param);
            }

            // then
            var fastDrill = param.SkillReamerParameters
                .Select(x => x.SecondPreparedHoleDiameter)
                .FirstOrDefault();
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            Assert.AreEqual($"穴径に該当するリストがありません 穴径: {fastDrill}",
                ex.Message);
        }

        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 1400)]
        [DataRow(MaterialType.Iron, 1100)]
        public void 正常系_面取りプログラムがリーマパラメータで書き換えられること(MaterialType material, int expectedSpin)
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create(material: material);

            IMainProgramParameterRewriter skillReamingParameterRewriter = new SkillReamingParameterRewriter();
            var actual = skillReamingParameterRewriter.RewriteByTool(param);

            // then
            decimal rewritedSpin = NCWordから値を取得する(actual, 'S');
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NCWordから値を取得する(actual, 'Z');
            decimal? expectedChamferingDepth = param.SkillReamerParameters
                .Select(x => x.ChamferingDepth)
                .FirstOrDefault();
            Assert.AreEqual(expectedChamferingDepth, rewritedDepth, "Z値");
        }

        [TestMethod]
        public void 正常系_面取りプログラムが無いパラメータで書き換えをしたとき何もしないこと()
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create(skillReamerParameters: new List<ReamingProgramPrameter>
            {
                new("200", 10m, 20m, 0.1m, null),
            });
            IMainProgramParameterRewriter skillReamingParameterRewriter = new SkillReamingParameterRewriter();
            var actual = skillReamingParameterRewriter.RewriteByTool(param);

            // then
            Assert.AreEqual(0, actual.Count());
        }

        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 10.5, 1130, 140)]
        [DataRow(MaterialType.Iron, 12.4, 360, 40)]
        public void 正常系_リーマプログラムがリーマパラメータで書き換えられること(MaterialType material, double expectedThickness, int expectedSpin, int expectedFeed)
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create(material: material, thickness: (decimal)expectedThickness);
            IMainProgramParameterRewriter skillReamingParameterRewriter = new SkillReamingParameterRewriter();
            var actual = skillReamingParameterRewriter.RewriteByTool(param);

            // then
            decimal rewritedSpin = NCWordから値を取得する(actual, 'S');
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NCWordから値を取得する(actual, 'Z');
            Assert.AreEqual((decimal)-expectedThickness - 5m, rewritedDepth, "Z値");

            decimal rewritedFeed = NCWordから値を取得する(actual, 'F');
            Assert.AreEqual(expectedFeed, rewritedFeed, "送り");
        }
    }
}