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
using ModuleModel;
using ModuleEffects;

namespace ModuleObject
{
    public class ObjectProvider
    {
        #region Attributes
        List<CBuilding> lstBuildings = new List<CBuilding>();
        List<CCharacterObject> lstCharacters = new List<CCharacterObject>();
        GraphicsDevice device;
        ContentManager content;
        float buildingScale = 6.00f;
        float characterScale = 1.03f;
        CBuilding building0;
        CBuilding building1;
        #endregion Attributes

        #region Constructor
        public ObjectProvider(GraphicsDevice device, ContentManager content)
        {
            Initialize(device);
            LoadContent(content);
        }
        #endregion Constructor

        #region Initialize
        private void Initialize(GraphicsDevice device)
        {
            this.device = device;
        }

        

        private void LoadBuildings(ContentManager content)
        {
            #region Declaration
            string[] strModelAssetPaths = 
            {
                @"Models\Buildings\mainbuilding\mainbuilding",
                @"Models\Buildings\japan_consulate\japan_consulate",
                @"Models\Buildings\japan_dojo\japan_dojo",
                @"Models\Buildings\japan_towncenter\japan_towncenter",
                @"Models\Buildings\porcelain_tower\porcelain_tower",
                @"Models\Buildings\summer_palace\summer_palace",
                @"Models\Buildings\japan_monastery\japan_monastery"

                //@"Models\Buildings\summer_palace\summer_palace",
                //@"Models\Buildings\japan_monastery\japan_monastery",

                /*@"Models\Buildings\mainbuilding\mainbuilding",
                @"Models\Buildings\china_circlebuilding\china_circlebuilding",
                @"Models\Buildings\japan_circlebuilding\japan_circlebuilding",
                @"Models\Buildings\tallbuilding\tallbuilding",
                @"Models\Buildings\japan_boatbuilding\japan_boatbuilding",

                @"Models\Buildings\china_boatbuilding\china_boatbuilding",
                @"Models\Buildings\china_waterbuilding\china_waterbuilding",
                @"Models\Buildings\japan_waterbuilding\japan_waterbuilding",
                @"Models\Buildings\topbuilding\topbuilding",
                @"Models\Buildings\imperial_palace\imperial_palace"*/
            };
            #endregion Declaration

            //building0 = new CBuilding();
            //building0.InitializeObject(device, content, strModelAssetPaths[0], Vector3.Zero, buildingScale, 0);

            //building1 = new CBuilding();
            //building1.InitializeObject(device, content, strModelAssetPaths[1], Vector3.Zero, buildingScale, 0);

            //xbuilding = building1;
            
            for (int i = 0; i < strModelAssetPaths.Length; i++)
            {
                CBuilding building = new CBuilding();

                building.InitializeObject(device, content, strModelAssetPaths[i], Vector3.Zero, buildingScale, 0);
                
                lstBuildings.Add(building);
            }

            xbuilding = lstBuildings[0];
        }

        private void LoadCharacters(ContentManager content)
        {
            #region Declaration
            string[] strModelAssetPaths = 
            {
                @"Models\Army\BodyModels\dragonknight_dragon",
                @"Models\Army\BodyModels\knight",
                @"Models\Army\BodyModels\forgottenone",
                @"Models\Army\BodyModels\forgottentwo",
                @"Models\Army\BodyModels\hunter",
                @"Models\Army\BodyModels\linfang",
                @"Models\Army\BodyModels\ryla",
                @"Models\Army\BodyModels\ddz",
                @"Models\Army\BodyModels\hakiru",
                @"Models\Army\BodyModels\terminator"
            };

            string[] strTextureAssetPaths = 
            {
                @"Images\Army\BodyTextures\dragonknight_dragon_red",
                @"Images\Army\BodyTextures\knight",
                @"Images\Army\BodyTextures\forgotten_blue",
                @"Images\Army\BodyTextures\forgotten_green",
                @"Images\Army\BodyTextures\hunterred",
                @"Images\Army\BodyTextures\linfangred",
                @"Images\Army\BodyTextures\rylared",
                @"Images\Army\BodyTextures\ddz_blue",
                @"Images\Army\BodyTextures\hakiru",
                @"Images\Army\BodyTextures\terminator_ultra"
            };

            string[] strWModelAssetPaths =
            {
                @"Models\Army\WeaponModels\dragonknight_knight",
                @"Models\Army\WeaponModels\lightningSword",
                @"Models\Army\WeaponModels\forgotten_weapon",
                @"Models\Army\WeaponModels\forgotten_weapon",
                @"Models\Army\WeaponModels\hunter_weapon",
                @"Models\Army\WeaponModels\linfang_weapon",
                @"Models\Army\WeaponModels\ryla_weapon",
                @"Models\Army\WeaponModels\ddz_weapon",
                @"Models\Army\WeaponModels\hakiru_weapon",
                @"Models\Army\WeaponModels\terminator_weapon"
            };

            string[] strWTextureAssetPaths =
            {
                @"Images\Army\WeaponTextures\dragonknight_knight",
                @"Images\Army\WeaponTextures\lightningSword",
                @"Images\Army\WeaponTextures\forgotten_weapon",
                @"Images\Army\WeaponTextures\forgotten_weapon",
                @"Images\Army\WeaponTextures\hunter_weapon",
                @"Images\Army\WeaponTextures\linfang_weapon",
                @"Images\Army\WeaponTextures\ryla_weapon",
                @"Images\Army\WeaponTextures\ddz_weapon",
                @"Images\Army\WeaponTextures\hakiru_weapon",
                @"Images\Army\WeaponTextures\terminator_weapon"
            };

            string[] strAnimationFiles = 
            {
                @"Content\MD2Animation2.xml",
                @"Content\MD2Animation1.xml",
                @"Content\MD2Animation2.xml",
                @"Content\MD2Animation2.xml",
                @"Content\MD2Animation3.xml",
                @"Content\MD2Animation2.xml",
                @"Content\MD2Animation1.xml",
                @"Content\MD2Animation2.xml",
                @"Content\MD2Animation2.xml",
                @"Content\MD2Animation1.xml"
            };
            #endregion Declaration

            for (int i = 0; i < strModelAssetPaths.Length; i++)
            {
                CCharacterObject character = new CCharacterObject();

                character.InitializeObject(
                    device, 
                    content, 
                    strModelAssetPaths[i], 
                    strTextureAssetPaths[i],
                    strWModelAssetPaths[i],
                    strWTextureAssetPaths[i],
                    strAnimationFiles[i],
                    Vector3.Zero, characterScale, 25f);

                lstCharacters.Add(character);
            }
        }

        private void LoadContent(ContentManager content)
        {
            this.content = content;
            LoadBuildings(content);
            LoadCharacters(content);
        }
        #endregion Initialize

        #region Service Methods
        private CCharacterObject CreateCharacter(int index)
        {
            #region Declaration
            string[] strModelAssetPaths = 
            {
                @"Models\Army\BodyModels\dragonknight_dragon",
                @"Models\Army\BodyModels\knight",
                @"Models\Army\BodyModels\forgottenone",
                @"Models\Army\BodyModels\forgottentwo",
                @"Models\Army\BodyModels\hunter",
                @"Models\Army\BodyModels\linfang",
                @"Models\Army\BodyModels\ryla",
                @"Models\Army\BodyModels\ddz",
                @"Models\Army\BodyModels\hakiru",
                @"Models\Army\BodyModels\terminator"
            };

            string[] strTextureAssetPaths = 
            {
                @"Images\Army\BodyTextures\dragonknight_dragon_red",
                @"Images\Army\BodyTextures\knight",
                @"Images\Army\BodyTextures\forgotten_blue",
                @"Images\Army\BodyTextures\forgotten_green",
                @"Images\Army\BodyTextures\hunterred",
                @"Images\Army\BodyTextures\linfangred",
                @"Images\Army\BodyTextures\rylared",
                @"Images\Army\BodyTextures\ddz_blue",
                @"Images\Army\BodyTextures\hakiru",
                @"Images\Army\BodyTextures\terminator_ultra"
            };

            string[] strWModelAssetPaths =
            {
                @"Models\Army\WeaponModels\dragonknight_knight",
                @"Models\Army\WeaponModels\lightningSword",
                @"Models\Army\WeaponModels\forgotten_weapon",
                @"Models\Army\WeaponModels\forgotten_weapon",
                @"Models\Army\WeaponModels\hunter_weapon",
                @"Models\Army\WeaponModels\linfang_weapon",
                @"Models\Army\WeaponModels\ryla_weapon",
                @"Models\Army\WeaponModels\ddz_weapon",
                @"Models\Army\WeaponModels\hakiru_weapon",
                @"Models\Army\WeaponModels\terminator_weapon"
            };

            string[] strWTextureAssetPaths =
            {
                @"Images\Army\WeaponTextures\dragonknight_knight",
                @"Images\Army\WeaponTextures\lightningSword",
                @"Images\Army\WeaponTextures\forgotten_weapon",
                @"Images\Army\WeaponTextures\forgotten_weapon",
                @"Images\Army\WeaponTextures\hunter_weapon",
                @"Images\Army\WeaponTextures\linfang_weapon",
                @"Images\Army\WeaponTextures\ryla_weapon",
                @"Images\Army\WeaponTextures\ddz_weapon",
                @"Images\Army\WeaponTextures\hakiru_weapon",
                @"Images\Army\WeaponTextures\terminator_weapon"
            };

            string[] strAnimationFiles = 
            {
                @"Content\MD2Animation2.xml",
                @"Content\MD2Animation1.xml",
                @"Content\MD2Animation2.xml",
                @"Content\MD2Animation2.xml",
                @"Content\MD2Animation3.xml",
                @"Content\MD2Animation2.xml",
                @"Content\MD2Animation1.xml",
                @"Content\MD2Animation2.xml",
                @"Content\MD2Animation2.xml",
                @"Content\MD2Animation1.xml"
            };
            #endregion Declaration

            CCharacterObject character = new CCharacterObject();

            character.InitializeObject(
                device,
                content,
                strModelAssetPaths[index],
                strTextureAssetPaths[index],
                strWModelAssetPaths[index],
                strWTextureAssetPaths[index],
                strAnimationFiles[index],
                Vector3.Zero, characterScale, 25f);

            return character;
        }
        CBuilding xbuilding;
        public void SetActive(int index)
        {
            xbuilding = lstBuildings[index];
        }

        public CBuilding GetBuildingAt(int index)
        {
            //if (index < 0 || index > lstBuildings.Count - 1)
            //{
            //    return null;
            //}
            return xbuilding;
        }

        public CCharacterObject GetCharacterAt(int index)
        {
            if (index < 0 || index > lstCharacters.Count - 1)
            {
                return null;
            }
            return CreateCharacter(index);
        }
        #endregion Service Methods
    }
}
