using System.Collections.Generic;
using FixedMath;
using RetroECS;

namespace GalacticMarauders
{
    /*
          The frame enum
      */
    public enum Images : ushort
    {
        _null = 0,
        _null_persist,
        enemy_00_a,
        enemy_01_a,
        enemy_02_a,
        enemy_03_a,
        enemy_04_a,
        enemy_05_a,
        enemy_06_a,
        enemy_07_a,
        enemy_08_a,
        enemy_09_a,
        enemy_10_a,
        enemy_00_b,
        enemy_01_b,
        enemy_02_b,
        enemy_03_b,
        enemy_04_b,
        enemy_05_b,
        enemy_06_b,
        enemy_07_b,
        enemy_08_b,
        enemy_09_b,
        enemy_10_b,
        player_ship_0,
        player_ship_1,
        player_shot,
        enemy_shot,
        easy_0,
        easy_1,
        player_boom_0,
        player_boom_1,
        player_boom_2,
        player_boom_3,
        player_boom_4,
        player_boom_5,
        player_boom_6,
        enemy_boom_0,
        enemy_boom_1,
        enemy_boom_2,
        enemy_boom_3,
        enemy_boom_4,
        enemy_boom_5,
        enemy_boom_6,
        local_player_0,
        local_player_1,
        text_ready,
        text_no,
        text_great,
        target
    }

    public class Data
    {
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
        public Dictionary<ObjType, Cp.Prefab> Objects = new Dictionary<ObjType, Cp.Prefab>
    {
        {
            ObjType.ShotCleaner,
            new Cp.Prefab
            {
                components = Cp.Active | Cp.Component | Cp.Body | Cp.ObjectId,
                objectId = (byte)ObjType.ShotCleaner,
                body = new Body
                {
                    velocity = new Vector2(0,0),
                    size = new Vector2(960, 540)
                }
            }
        },
        {
            ObjType.Player,
            new Cp.Prefab
            {
                components = Cp.Active | Cp.Component | Cp.Body | Cp.ObjectId | Cp.Player | Cp.Animator,
                objectId = (byte)ObjType.Player,
                animator = new Animator
                {
                    frame = (ushort)Images.player_ship_0,
                    count = 0
                },
                player = new Player
                {
                    damage = 0
                },
                body = new Body
                {
                    velocity = new Vector2(0, 0),
                    size = new Vector2(16, 10)
                }
            }
        },
        {
            ObjType.Enemy,
            new Cp.Prefab
            {
                components = Cp.Active | Cp.Component | Cp.Body | Cp.ObjectId | Cp.Enemy | Cp.Animator,
                objectId = (byte) ObjType.Enemy,
                enemy = new Enemy
                {
                    target = Cp.Handle.Null,
                    direction = 1,
                    counter = 0
                },
                animator = new Animator
                {
                    frame = (ushort)Images.player_ship_0,
                    count = 0
                },
                body = new Body
                {
                    velocity = new Vector2(0, 0),
                    size = new Vector2(16, 10)
                }
            }
        },
        {
            ObjType.Bullet,
            new Cp.Prefab
            {
                components = Cp.Active | Cp.Component | Cp.Body | Cp.ObjectId | Cp.Animator,
                objectId = (byte) ObjType.Bullet,
                animator = new Animator
                {
                    frame = (ushort)Images.player_shot,
                    count = 0
                },
                body = new Body
                {
                    velocity = new Vector2(0, 16),
                    size = new Vector2(12, 20)
                }
            }
        },
        {
            ObjType.BadBullet,
            new Cp.Prefab
            {
                components = Cp.Active | Cp.Component | Cp.Body | Cp.ObjectId | Cp.Animator,
                objectId = (byte) ObjType.BadBullet,
                animator = new Animator
                {
                    frame = (ushort)Images.enemy_shot,
                    count = 0
                },
                body = new Body
                {
                    velocity = new Vector2(0, -8),
                    size = new Vector2(7, 7)
                }
            }
        },
        {
            ObjType.Boom,
            new Cp.Prefab
            {
                components = Cp.Active | Cp.Component | Cp.Body | Cp.ObjectId | Cp.Animator,
                objectId = (byte)ObjType.Boom,
                animator = new Animator
                {
                    frame = (ushort)Images.enemy_boom_0,
                    count = 0
                },
                body = new Body
                {
                    velocity = new Vector2(0, 0),
                    size = new Vector2(14, 14)
                }
            }
        },
        {
            ObjType.PlayerBoom,
            new Cp.Prefab
            {
                components = Cp.Active | Cp.Component | Cp.Body | Cp.ObjectId | Cp.Animator,
                objectId = (byte)ObjType.PlayerBoom,
                animator = new Animator
                {
                    frame = (ushort)Images.player_boom_0,
                    count = 0
                },
                body = new Body
                {
                    velocity = new Vector2(0, 0),
                    size = new Vector2(22, 21)
                }
            }
        }
    };
    }


    // (TextAsset)Resources.Load("Data\\parameters.json")
    public static class Static
    {
        public static Data Const = new Data();
    }

}