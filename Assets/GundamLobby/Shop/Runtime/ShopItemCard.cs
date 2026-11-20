using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace GundamLobby
{
 [RequireComponent(typeof(Button))]public class ShopItemCard:MonoBehaviour
 {
  public ShopController owner;public int dataIndex;
  public TMP_Text titleLabel,priceLabel,priceHeading,unitPriceLabel,timeLabel,bidsLabel,tradeLabel,timeHeading,bidsHeading;
  public Image icon;public ShopArtwork frameArt,auctionArt;public LobbyShape rarityMark;
  ItemArtwork art;TMP_Text tier;LobbyHeaderIcon currencyIcon;
  void Awake(){GetComponent<Button>().onClick.AddListener(()=>owner.ShowItem(dataIndex));}
  public void Bind(ShopItemDefinition item,int index)
  {
   bool skin=owner.IsSkin(item),gundam=item.code.StartsWith("gundam:"),unique=skin||gundam,owned=owner.Owns(item);dataIndex=index;
   titleLabel.text=item.title;titleLabel.fontSizeMax=23;titleLabel.fontSizeMin=17;titleLabel.color=new Color(.85f,.91f,.95f);
   priceHeading.text=gundam&&owned?"STATUS":"PRICE";priceLabel.text=gundam&&owned?"OWNED":owner.Price(item).ToString("N0");priceLabel.color=skin?ProgressionUi.Cyan:new Color(.88f,.75f,.48f);
   var heading=transform.Find("Unit price heading");if(heading)heading.GetComponent<TMP_Text>().text=unique?"LICENSE":"OWNED";
   unitPriceLabel.text=unique?(owned?"UNLOCKED":"AVAILABLE"):PlayerProgress.Count(item.code).ToString("N0");timeHeading.text=skin?"CURRENCY":gundam?"ROLE":"FUNCTION";
   var definition=GameItems.Find(item.code);timeLabel.text=skin?"CRYSTALS":gundam?"PLAYABLE":GameItems.Summary(definition);
   bidsLabel.gameObject.SetActive(false);bidsHeading.gameObject.SetActive(false);auctionArt.gameObject.SetActive(false);frameArt.gameObject.SetActive(false);tradeLabel.text=owned?"OWNED":skin?"COSMETIC":"";tradeLabel.color=new Color(.43f,.61f,.69f);
   icon.gameObject.SetActive(unique);icon.sprite=owner.Portrait(item);icon.preserveAspect=true;icon.color=skin?GundamSkinBinding.Tint(owner.SkinIndex(item)):Color.white;icon.rectTransform.anchoredPosition=new Vector2(12,-16);icon.rectTransform.sizeDelta=new Vector2(260,144);
   if(!art){art=ProgressionUi.Rect("Rendered equipment",icon.transform.parent,14,2,256,161).gameObject.AddComponent<ItemArtwork>();tier=ProgressionUi.Text("Rarity tier","",icon.transform.parent,titleLabel.font,12,8,110,22,13,Color.white);}
   art.gameObject.SetActive(!unique);if(!unique)art.Show(item.code);tier.text=owner.RarityName(item.rarity);tier.color=owner.RarityColor(item.rarity);
   rarityMark.geometry=LobbyShape.Geometry.Box;rarityMark.rectTransform.anchoredPosition=Vector2.zero;rarityMark.rectTransform.sizeDelta=new Vector2(2,199);rarityMark.color=owner.RarityColor(item.rarity);rarityMark.SetVerticesDirty();
   var grid=icon.transform.parent.Find("Artwork grid");if(grid)grid.gameObject.SetActive(false);
   if(!currencyIcon){var t=transform.Find("Credits icon");if(t)currencyIcon=t.GetComponent<LobbyHeaderIcon>();}
   if(currencyIcon){currencyIcon.symbol=skin?LobbyHeaderIcon.Symbol.Crystal:LobbyHeaderIcon.Symbol.Credits;currencyIcon.color=priceLabel.color;currencyIcon.SetVerticesDirty();}
   var hover=GetComponent<LobbyHover>();if(hover){hover.normal=new Color(.025f,.047f,.071f,.98f);hover.hover=new Color(.035f,.10f,.145f,.99f);hover.pressed=new Color(.04f,.17f,.23f,.99f);}
  }
 }
}