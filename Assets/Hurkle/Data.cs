using System.Collections.Generic;
using System.Linq.Expressions;
using FixedMath;
using RetroECS;
using Games;

namespace Hurkle
{
    // ascii8_253, ascii8_254
    public enum Images : ushort
    {
        ascii8_253,
        ascii8_0,
        ascii8_1,
        ascii8_2,
        ascii8_3,
        ascii8_4,
        ascii8_5,
        ascii8_6,
        ascii8_7,
        ascii8_8,
        ascii8_9,
        ascii8_10,
        ascii8_11,
        ascii8_12,
        ascii8_13,
        ascii8_14,
        ascii8_15,
        ascii8_16,
        ascii8_17,
        ascii8_18,
        ascii8_19,
        ascii8_20,
        ascii8_21,
        ascii8_22,
        ascii8_23,
        ascii8_24,
        ascii8_25,
        ascii8_26,
        ascii8_27,
        ascii8_28,
        ascii8_29,
        ascii8_30,
        ascii8_254,
        ascii8_31,
        ascii8_32,
        ascii8_33,
        ascii8_34,
        ascii8_35,
        ascii8_36,
        ascii8_37,
        ascii8_38,
        ascii8_39,
        ascii8_40,
        ascii8_41,
        ascii8_42,
        ascii8_43,
        ascii8_44,
        ascii8_45,
        ascii8_46,
        ascii8_47,
        ascii8_48,
        ascii8_49,
        ascii8_50,
        ascii8_51,
        ascii8_52,
        ascii8_53,
        ascii8_54,
        ascii8_55,
        ascii8_56,
        ascii8_57,
        ascii8_58,
        ascii8_59,
        ascii8_60,
        ascii8_61,
        ascii8_62,
        ascii8_63,
        ascii8_64,
        ascii8_65,
        ascii8_66,
        ascii8_67,
        ascii8_68,
        ascii8_69,
        ascii8_70,
        ascii8_71,
        ascii8_72,
        ascii8_73,
        ascii8_74,
        ascii8_75,
        ascii8_76,
        ascii8_77,
        ascii8_78,
        ascii8_79,
        ascii8_80,
        ascii8_81,
        ascii8_82,
        ascii8_83,
        ascii8_84,
        ascii8_85,
        ascii8_86,
        ascii8_87,
        ascii8_88,
        ascii8_89,
        ascii8_90,
        ascii8_91,
        ascii8_92,
        ascii8_93,
        ascii8_94,
        ascii8_95,
        ascii8_96,
        ascii8_97,
        ascii8_98,
        ascii8_99,
        ascii8_100,
        ascii8_101,
        ascii8_102,
        ascii8_103,
        ascii8_104,
        ascii8_105,
        ascii8_106,
        ascii8_107,
        ascii8_108,
        ascii8_109,
        ascii8_110,
        ascii8_111,
        ascii8_112,
        ascii8_113,
        ascii8_114,
        ascii8_115,
        ascii8_116,
        ascii8_117,
        ascii8_118,
        ascii8_119,
        ascii8_120,
        ascii8_121,
        ascii8_122,
        ascii8_123,
        ascii8_124,
        ascii8_125,
        ascii8_126,
        ascii8_127,
        ascii8_128,
        ascii8_129,
        ascii8_130,
        ascii8_131,
        ascii8_132,
        ascii8_133,
        ascii8_134,
        ascii8_135,
        ascii8_136,
        ascii8_137,
        ascii8_138,
        ascii8_139,
        ascii8_140,
        ascii8_141,
        ascii8_142,
        ascii8_143,
        ascii8_144,
        ascii8_145,
        ascii8_146,
        ascii8_147,
        ascii8_148,
        ascii8_149,
        ascii8_150,
        ascii8_151,
        ascii8_152,
        ascii8_153,
        ascii8_154,
        ascii8_155,
        ascii8_156,
        ascii8_157,
        ascii8_158,
        ascii8_159,
        ascii8_160,
        ascii8_161,
        ascii8_162,
        ascii8_163,
        ascii8_164,
        ascii8_165,
        ascii8_166,
        ascii8_167,
        ascii8_168,
        ascii8_169,
        ascii8_170,
        ascii8_171,
        ascii8_172,
        ascii8_173,
        ascii8_174,
        ascii8_175,
        ascii8_176,
        ascii8_177,
        ascii8_178,
        ascii8_179,
        ascii8_180,
        ascii8_181,
        ascii8_182,
        ascii8_183,
        ascii8_184,
        ascii8_185,
        ascii8_186,
        ascii8_187,
        ascii8_188,
        ascii8_189,
        ascii8_190,
        ascii8_191,
        ascii8_192,
        ascii8_193,
        ascii8_194,
        ascii8_195,
        ascii8_196,
        ascii8_197,
        ascii8_198,
        ascii8_199,
        ascii8_200,
        ascii8_201,
        ascii8_202,
        ascii8_203,
        ascii8_204,
        ascii8_205,
        ascii8_206,
        ascii8_207,
        ascii8_208,
        ascii8_209,
        ascii8_210,
        ascii8_211,
        ascii8_212,
        ascii8_213,
        ascii8_214,
        ascii8_215,
        ascii8_216,
        ascii8_217,
        ascii8_218,
        ascii8_219,
        ascii8_220,
        ascii8_221,
        ascii8_222,
        ascii8_223,
        ascii8_224,
        ascii8_225,
        ascii8_226,
        ascii8_227,
        ascii8_228,
        ascii8_229,
        ascii8_230,
        ascii8_231,
        ascii8_232,
        ascii8_233,
        ascii8_234,
        ascii8_235,
        ascii8_236,
        ascii8_237,
        ascii8_238,
        ascii8_239,
        ascii8_240,
        ascii8_241,
        ascii8_242,
        ascii8_243,
        ascii8_244,
        ascii8_245,
        ascii8_246,
        ascii8_247,
        ascii8_248,
        ascii8_249,
        ascii8_250,
        ascii8_251,
        ascii8_252,
        ascii8_255
    }



    public class Data
    {
        public Fixture[] MapFixtures = new Fixture[]
        {
            new Fixture
            {
                flags = Fixture.fEmpty,
                size = new Vector2(0,0)
            },
            new Fixture
            {
                size = new Vector2(16,16)
            },
            new Fixture
            {
                flags = Fixture.fEmpty,
                size = new Vector2(0,0)
            }
        };

        public Vector2[] FlowDirections = new Vector2[]
        {
            new Vector2( 0, 0),
            new Vector2( 1, 0).NormalizedCopy(),
            new Vector2( 1, 1).NormalizedCopy(),
            new Vector2( 0, 1).NormalizedCopy(),
            new Vector2(-1, 1).NormalizedCopy(),
            new Vector2(-1, 0).NormalizedCopy(),
            new Vector2(-1,-1).NormalizedCopy(),
            new Vector2( 0,-1).NormalizedCopy(),
            new Vector2( 1,-1).NormalizedCopy()
        };

        public Vector<Vector2> EnemySpawn;
        public byte[] FlowData;
        public Vector2 PlayerStart;
        public int MapDataPitch;
        public ushort[] MapData;

        /*
            It may be that these floating point conversions may differ across machines,
            so actually what should happen is these values should be saved and reloaded,
            or have a table generated to keep them consistent across machines.
        */

        // decimal
        public Scaler v0_22 = (Scaler)0.22f;
        public Scaler v0_1 = (Scaler)0.1f;
        public Scaler v0_04 = (Scaler)0.04f;
        public Scaler v0_02333333333333333 = (Scaler)0.02333333333333333f;
        public Scaler v0_00005 = (Scaler)0.00005f;
        public Scaler v0_02 = (Scaler)0.02f;
        public Scaler v0_2 = (Scaler)0.2f;
        public Scaler v0_15 = (Scaler)0.15f;
        public Scaler v0_003 = (Scaler)0.003f;
        public Scaler v0_3 = (Scaler)0.3f;
        public Scaler v0_03 = (Scaler)0.03f;
        public Scaler v0_062 = (Scaler)0.062f;
        public Scaler v0_25 = (Scaler)0.25f;
        public Scaler v0_01111111111111111 = (Scaler)0.01111111111111111f;
        public Scaler v0_00833333333333333 = (Scaler)0.00833333333333333f;

        // fractions
        public Scaler v1o100 = (Scaler)(1.0f / 100.0f);

        // misc
        public ushort EnemyCount = 11;

        // this can all become json files
        public Dictionary<ObjType, Cp.Prefab> Objects;

    }


    // (TextAsset)Resources.Load("Data\\parameters.json")
    public static class Static
    {
        public static Data Const = null;
    }




}

