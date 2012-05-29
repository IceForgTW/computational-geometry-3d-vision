using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using ModuleObject;
using ModuleEffects;
using ModuleEffectController;

namespace ModuleObject
{
    public class EffectCreator
    {
        public enum BuildingHaloColor
        {
            Blue, Green, Yellow, White, Violet
        };

        public enum BuildingHaloStyle
        {
            Style01, Style02
        };

        public enum CharacterHaloColor
        {
            Blue, Green, Yellow, White, Violet
        };

        public enum CharacterHaloStyle
        {
            Style01, Style02
        }


        static float s = 0.00005f;
        public static MyEffect CreateHaloForBuilding(Game game, CBuilding building, BuildingHaloStyle style, BuildingHaloColor color)
        {
            IHaloController haloController;
            MyHalo halo;
            

            switch (style)
            {
                case BuildingHaloStyle.Style01:
                    BuildingHaloController_Style01 haloController1;
                    haloController1 = new BuildingHaloController_Style01();
                    haloController1.Scale = s;
                    haloController1.MaxHeight = building.Radius;
                    haloController = haloController1;
                    break;
                case BuildingHaloStyle.Style02:
                default:
                    BuildingHaloController_Style02 haloController2;
                    haloController2 = new BuildingHaloController_Style02();
                    haloController2.Scale = s;
                    haloController = haloController2;
                    break;
            }

            switch (color)
            {
                case BuildingHaloColor.Blue:
                default:
                    halo = new HaloBlue(game, haloController);
                    halo.Initialize();
                    halo.Radius = building.Radius;
                    halo.Position = building.Position;
                    break;
                case BuildingHaloColor.Green:
                    halo = new HaloGreen(game, haloController);
                    halo.Initialize();
                    halo.Radius = building.Radius;
                    halo.Position = building.Position;
                    break;
                case BuildingHaloColor.Yellow:
                    halo = new HaloYellow(game, haloController);
                    halo.Initialize();
                    halo.Radius = building.Radius;
                    halo.Position = building.Position;
                    break;
                case BuildingHaloColor.White:
                    halo = new HaloWhite(game, haloController);
                    halo.Initialize();
                    halo.Radius = building.Radius;
                    halo.Position = building.Position;
                    break;
                case BuildingHaloColor.Violet:
                    halo = new HaloViolet(game, haloController);
                    halo.Initialize();
                    halo.Radius = building.Radius;
                    halo.Position = building.Position;
                    break;
            }

            return halo;
        }

        public static MyEffect CreateHaloForCharacter(Game game, CCharacterObject character, CharacterHaloStyle style, CharacterHaloColor color)
        {
            IHaloController haloController;
            MyHalo halo;

            switch (style)
            {
                case CharacterHaloStyle.Style01:
                    CharacterHaloController_Style01 haloController1;
                    haloController1 = new CharacterHaloController_Style01();
                    haloController1.Scale = s;
                    haloController1.MaxHeight = character.Radius * 2;
                    haloController = haloController1;
                    break;
                case CharacterHaloStyle.Style02:
                default:
                    CharacterHaloController_Style02 haloController2;
                    haloController2 = new CharacterHaloController_Style02();
                    haloController2.Scale = s;
                    haloController = haloController2;
                    break;
            }
            Vector3 pos;
            switch (color)
            {
                case CharacterHaloColor.Blue:
                default:
                    halo = new HaloBlue(game, haloController);
                    halo.Initialize();
                    halo.Radius = character.Radius * 2;
                    pos = character.Position;
                    pos.Y -= character.Radius;
                    halo.Position = pos;
                    break;
                case CharacterHaloColor.Green:
                    halo = new HaloGreen(game, haloController);
                    halo.Initialize();
                    halo.Radius = character.Radius * 2;
                    pos = character.Position;
                    pos.Y -= character.Radius;
                    halo.Position = pos;
                    break;
                case CharacterHaloColor.Yellow:
                    halo = new HaloYellow(game, haloController);
                    halo.Initialize();
                    halo.Radius = character.Radius * 2;
                    pos = character.Position;
                    pos.Y -= character.Radius;
                    halo.Position = pos;
                    break;
                case CharacterHaloColor.White:
                    halo = new HaloWhite(game, haloController);
                    halo.Initialize();
                    halo.Radius = character.Radius * 2;
                    pos = character.Position;
                    pos.Y -= character.Radius;
                    halo.Position = pos;
                    break;
                case CharacterHaloColor.Violet:
                    halo = new HaloViolet(game, haloController);
                    halo.Initialize();
                    halo.Radius = character.Radius * 2;
                    pos = character.Position;
                    pos.Y -= character.Radius;
                    halo.Position = pos;
                    break;
            }
            
            return halo;
        }
    }
}
