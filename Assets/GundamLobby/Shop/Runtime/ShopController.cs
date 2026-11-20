using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GundamLobby
{
    [DefaultExecutionOrder(230)]
    public class ShopController:MonoBehaviour
    {
        public HomeLobbyDashboard dashboard;
        public ShopCatalog catalog;
        public GameObject shopCanvas,filterPanel,detailPanel,emptyState;
        public ShopItemCard[] cards;
        public Button[] categoryButtons,tradeButtons,rarityButtons;
        public Button listingButton,bidsButton,previousButton,nextButton;
        public TMP_InputField searchField;
        public TMP_Text resultLabel,pageLabel,sortLabel,filterLabel,viewLabel;
        public TMP_Text detailTitle,detailCategory,detailDescription,detailPrice,detailMeta,detailActionLabel;
        public ShopArtwork detailFrame;
        public Image detailIcon;
        public CanvasGroup toast;
        public TMP_Text toastLabel;
        public int SelectedCategory{get;private set;}
        public int TradeTab{get;private set;}
        public int CurrentPage{get;private set;}
        public int RarityFilter{get;private set;}=-1;
        public int ListingView{get;private set;}
        public int FilteredCount=>filtered.Count;
        public int PageCount=>Mathf.Max(1,Mathf.CeilToInt(filtered.Count/10f));
        public bool IsOpen=>shopCanvas&&shopCanvas.activeSelf;
        public bool DetailOpen=>detailPanel&&detailPanel.activeSelf;
        readonly List<int> filtered=new List<int>();
        Button minus,plus;
        int purchaseQuantity=1,progressVersion=-1;ItemArtwork detailArtwork;TMP_Text quantityLabel;
        int sortMode,selectedItem,screenW,screenH;float toastUntil;bool lobbyWasEnabled,pivotWasActive;

        void Awake()
        {
            shopCanvas.SetActive(false);filterPanel.SetActive(false);detailPanel.SetActive(false);searchField.onValueChanged.AddListener(_=>Search());
            for(int i=0;i<categoryButtons.Length;i++){categoryButtons[i].gameObject.SetActive(i<catalog.categories.Length);if(i<catalog.categories.Length)categoryButtons[i].GetComponentInChildren<TMP_Text>().text=catalog.categories[i];}
            string[] tabs={"ALL","CREDITS","CRYSTALS"};for(int i=0;i<tradeButtons.Length;i++)tradeButtons[i].GetComponentInChildren<TMP_Text>().text=tabs[i];
            listingButton.GetComponentInChildren<TMP_Text>().text="TRANSACTIONS";bidsButton.GetComponentInChildren<TMP_Text>().text="ITEM GUIDE";
            detailArtwork=ProgressionUi.Rect("Detailed item art",detailIcon.transform.parent,30,88,250,242).gameObject.AddComponent<ItemArtwork>();detailArtwork.gameObject.SetActive(false);
            var p=detailIcon.transform.parent;
            minus=ProgressionUi.Button("Decrease Quantity","-",p,detailTitle.font,54,360,48,42,()=>ChangeQuantity(-1));
            quantityLabel=ProgressionUi.Text("Purchase Quantity","1",p,detailTitle.font,108,360,92,42,25,Color.white);quantityLabel.alignment=TextAlignmentOptions.Center;
            plus=ProgressionUi.Button("Increase Quantity","+",p,detailTitle.font,206,360,48,42,()=>ChangeQuantity(1));
        }
        public bool IsSkin(ShopItemDefinition item)=>item.code!=null&&item.code.StartsWith("skin:");
        public int SkinIndex(ShopItemDefinition item)=>IsSkin(item)?int.Parse(item.code.Substring(5)):0;
        public bool Owns(ShopItemDefinition item)=>IsSkin(item)?PlayerProgress.OwnsSkin(dashboard.ActiveUnit.id,SkinIndex(item)):item.code.StartsWith("gundam:")&&PlayerProgress.OwnsGundam(item.code.Substring(7));
        public string Currency(ShopItemDefinition item)=>IsSkin(item)?"CRYSTALS":"CREDITS";
        public int Price(ShopItemDefinition item)=>IsSkin(item)?PlayerProgress.Rules.SkinPrice(SkinIndex(item)):item.price;
        public Sprite Portrait(ShopItemDefinition item)=>IsSkin(item)?dashboard.ActiveUnit.CardPortrait:item.icon;
        void ChangeQuantity(int delta){if(catalog.items[selectedItem].code.StartsWith("gundam:")||IsSkin(catalog.items[selectedItem]))return;purchaseQuantity=Mathf.Clamp(purchaseQuantity+delta,1,99);ShowItem(selectedItem);}
        public void Open()
        {
            if(IsOpen)return;dashboard.OpenLaboratory();lobbyWasEnabled=dashboard.lobby.enabled;pivotWasActive=dashboard.lobby.modelPivot.gameObject.activeSelf;
            dashboard.lobby.enabled=false;dashboard.lobby.modelPivot.gameObject.SetActive(false);dashboard.striker.lobbyCanvas.SetActive(false);shopCanvas.SetActive(true);
            SelectedCategory=-1;TradeTab=0;CurrentPage=0;filterPanel.SetActive(false);detailPanel.SetActive(false);toast.alpha=0;toastUntil=0;Fit();RefreshResults();
        }
        public void Close()
        {
            if(!IsOpen)return;filterPanel.SetActive(false);detailPanel.SetActive(false);shopCanvas.SetActive(false);dashboard.lobby.enabled=lobbyWasEnabled;dashboard.lobby.modelPivot.gameObject.SetActive(pivotWasActive);dashboard.striker.lobbyCanvas.SetActive(true);
        }
        static string Fold(string input)
        {
            var result=new StringBuilder();foreach(char c in (input??"").Normalize(NormalizationForm.FormD))if(CharUnicodeInfo.GetUnicodeCategory(c)!=UnicodeCategory.NonSpacingMark)result.Append(c=='đ'?'d':c=='Đ'?'D':c);return result.ToString().ToLowerInvariant();
        }
        public void Search(){CurrentPage=0;RefreshResults();}
        public void SetCategory(int index){if(index<0||index>=catalog.categories.Length)return;SelectedCategory=index;TradeTab=0;ListingView=0;CurrentPage=0;RefreshResults();}
        public void SetTradeTab(int index){TradeTab=Mathf.Clamp(index,0,2);SelectedCategory=-1;CurrentPage=0;RefreshResults();}
        public void SetRarity(int rarity){RarityFilter=rarity;CurrentPage=0;filterPanel.SetActive(false);RefreshResults();}
        public void ShowMyListings(){LobbyServices.For(dashboard).Open("TRANSACTIONS");}
        public void ShowMyBids(){LobbyServices.For(dashboard).Open("ITEM GUIDE");}
        void SetListingView(int view){ListingView=view;SelectedCategory=-1;TradeTab=0;RarityFilter=-1;CurrentPage=0;searchField.SetTextWithoutNotify("");RefreshResults();}
        public void Sort(){sortMode=(sortMode+1)%3;CurrentPage=0;RefreshResults();}
        public void PreviousPage(){if(CurrentPage>0){CurrentPage--;RefreshResults();}}
        public void NextPage(){if(CurrentPage+1<PageCount){CurrentPage++;RefreshResults();}}
        public void ToggleFilter(){filterPanel.SetActive(!filterPanel.activeSelf);}
        public void CloseFilter(){filterPanel.SetActive(false);}
        public void ClearFilters(){RarityFilter=-1;TradeTab=0;sortMode=0;CurrentPage=0;searchField.SetTextWithoutNotify("");filterPanel.SetActive(false);RefreshResults();}
        public void RefreshResults()
        {
            progressVersion=PlayerProgress.Version;string query=Fold(searchField.text.Trim());IEnumerable<int> list=Enumerable.Range(0,catalog.items.Length).Where(i=>
            {
                var item=catalog.items[i];return (SelectedCategory<0||item.category==SelectedCategory)&&(TradeTab==0||(TradeTab==1&&!IsSkin(item))||(TradeTab==2&&IsSkin(item)))&&(RarityFilter<0||item.rarity==RarityFilter)&&(ListingView!=1||item.isMyListing)&&(ListingView!=2||item.isMyBid)&&(query.Length==0||Fold(item.title).Contains(query));
            });
            list=sortMode==1?list.OrderBy(i=>IsSkin(catalog.items[i])).ThenByDescending(i=>Price(catalog.items[i])):sortMode==2?list.OrderBy(i=>catalog.items[i].title):list.OrderBy(i=>IsSkin(catalog.items[i])).ThenBy(i=>Price(catalog.items[i]));
            filtered.Clear();filtered.AddRange(list);CurrentPage=Mathf.Clamp(CurrentPage,0,PageCount-1);
            for(int i=0;i<cards.Length;i++){int visibleIndex=CurrentPage*10+i;bool visible=visibleIndex<filtered.Count;cards[i].gameObject.SetActive(visible);if(visible)cards[i].Bind(catalog.items[filtered[visibleIndex]],filtered[visibleIndex]);}
            emptyState.SetActive(filtered.Count==0);resultLabel.text=filtered.Count+" ITEMS";pageLabel.text=(CurrentPage+1)+" / "+PageCount;PagingButton(previousButton,CurrentPage>0);PagingButton(nextButton,CurrentPage+1<PageCount);
            sortLabel.text=new[]{"PRICE LOW TO HIGH","PRICE HIGH TO LOW","BY NAME"}[sortMode];filterLabel.text=RarityFilter<0?"FILTER":"FILTER: "+RarityName(RarityFilter);
            viewLabel.text=ListingView==1?"MY LISTINGS":ListingView==2?"MY BIDS":SelectedCategory>=0?catalog.categories[SelectedCategory]:"ALL CATEGORIES";
            for(int i=0;i<categoryButtons.Length;i++)Style(categoryButtons[i],ListingView==0&&SelectedCategory==i);
            for(int i=0;i<tradeButtons.Length;i++)Style(tradeButtons[i],TradeTab==i);
            for(int i=0;i<rarityButtons.Length;i++)Style(rarityButtons[i],RarityFilter==i-1);
            Style(listingButton,ListingView==1);Style(bidsButton,ListingView==2);
        }
        void Style(Button b,bool selected){var hover=b.GetComponent<LobbyHover>();var shape=b.GetComponent<LobbyShape>();hover.normal=selected?new Color(.04f,.23f,.32f):new Color(.025f,.065f,.09f,.85f);shape.color=hover.normal;shape.border=selected?new Color(.125f,.785f,.965f):new Color(.13f,.27f,.33f);shape.SetVerticesDirty();}
        void PagingButton(Button b,bool enabled){b.interactable=enabled;var hover=b.GetComponent<LobbyHover>();hover.enabled=enabled;b.targetGraphic.color=enabled?hover.normal:new Color(.025f,.065f,.09f,.4f);b.GetComponentInChildren<TMP_Text>().color=enabled?new Color(.9f,.95f,1):new Color(.3f,.43f,.5f);}
        public Color RarityColor(int value){return value==2?new Color(.88f,.71f,.36f):value==1?new Color(.63f,.34f,.86f):new Color(.13f,.70f,.9f);}
        public string RarityName(int value){return new[]{"RARE","EPIC","LEGENDARY"}[Mathf.Clamp(value,0,2)];}
        public void ShowItem(int index)
        {
            if(index<0||index>=catalog.items.Length)return;selectedItem=index;var item=catalog.items[index];filterPanel.SetActive(false);detailPanel.SetActive(true);
            bool gundam=item.code.StartsWith("gundam:"),skin=IsSkin(item),unique=gundam||skin;var definition=GameItems.Find(item.code);bool owned=Owns(item);int qty=unique?1:purchaseQuantity;
            detailTitle.text=item.title;detailCategory.text=catalog.categories[item.category]+"  /  "+RarityName(item.rarity);detailDescription.text=item.description;
            detailDescription.rectTransform.sizeDelta=new Vector2(620,116);detailDescription.overflowMode=TextOverflowModes.Ellipsis;
            detailPrice.text=((long)Price(item)*qty).ToString("N0")+" "+Currency(item);detailPrice.color=skin?ProgressionUi.Cyan:new Color(.88f,.75f,.48f);
            detailMeta.text=skin?"FOR "+dashboard.ActiveUnit.displayName+"  /  "+(owned?"DESIGN OWNED":"COSMETIC ONLY · NO STAT BONUS"):gundam?(owned?"PERMANENTLY OWNED":"UNLOCKS A PLAYABLE MOBILE SUIT"):"IN STORAGE  "+PlayerProgress.Count(item.code)+"   /   QUANTITY  "+qty+"\n"+(definition!=null?definition.Action:"");
            quantityLabel.text=qty.ToString();minus.gameObject.SetActive(!unique);plus.gameObject.SetActive(!unique);quantityLabel.gameObject.SetActive(!unique);
            detailActionLabel.text=skin?(owned?"EQUIP DESIGN":"UNLOCK DESIGN"):owned?"OWNED":gundam?"BUY GUNDAM":"PURCHASE  x"+qty;
            detailFrame.gameObject.SetActive(false);detailIcon.gameObject.SetActive(unique);detailIcon.sprite=Portrait(item);detailIcon.preserveAspect=true;detailIcon.color=skin?GundamSkinBinding.Tint(SkinIndex(item)):Color.white;detailArtwork.gameObject.SetActive(!unique);if(!unique)detailArtwork.Show(item.code);
            foreach(var c in detailIcon.transform.parent.GetComponentsInChildren<LobbyHeaderIcon>(true))if(c.name=="Credits icon"){c.symbol=skin?LobbyHeaderIcon.Symbol.Crystal:LobbyHeaderIcon.Symbol.Credits;c.color=detailPrice.color;c.SetVerticesDirty();}
            int balance=skin?PlayerProgress.Crystals:PlayerProgress.Credits;var action=detailActionLabel.GetComponentInParent<Button>();action.interactable=skin&&owned||!owned&&balance>=(long)Price(item)*qty;
        }

        public void CloseDetail(){detailPanel.SetActive(false);}
        public void Trade()
        {
            var item=catalog.items[selectedItem];string message;
            if(IsSkin(item))
            {
             int index=SkinIndex(item);string id=dashboard.ActiveUnit.id;
             if(!PlayerProgress.OwnsSkin(id,index)&&!PlayerProgress.BuySkin(id,index,out message)){Notify(message);return;}
             PlayerProgress.EquipSkin(id,index);message="Design equipped for "+dashboard.ActiveUnit.displayName+".";
            }
            else if(item.code.StartsWith("gundam:"))PlayerProgress.BuyGundam(item.code.Substring(7),out message);
            else PlayerProgress.BuyItem(item.code,purchaseQuantity,out message);
            Notify(message);ShowItem(selectedItem);RefreshResults();
        }
        public void Notify(string message){toastLabel.text=message;toastUntil=Time.unscaledTime+3;}
        public void Comms(){LobbyServices.For(dashboard).Open("ACTIVITY");}
        void Fit(){screenW=Screen.width;screenH=Screen.height;shopCanvas.GetComponent<CanvasScaler>().matchWidthOrHeight=screenW/(float)Mathf.Max(1,screenH)>=16f/9f?1:0;}
        void Update()
        {
            if(!IsOpen)return;if(LobbyServices.BlocksInput)return;if(progressVersion!=PlayerProgress.Version){RefreshResults();if(DetailOpen)ShowItem(selectedItem);}if(screenW!=Screen.width||screenH!=Screen.height)Fit();toast.alpha=Mathf.MoveTowards(toast.alpha,Time.unscaledTime<toastUntil?1:0,Time.unscaledDeltaTime*7);
#if ENABLE_INPUT_SYSTEM
            if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame){if(DetailOpen)CloseDetail();else if(filterPanel.activeSelf)CloseFilter();else Close();}
#elif ENABLE_LEGACY_INPUT_MANAGER
            if(Input.GetKeyDown(KeyCode.Escape)){if(DetailOpen)CloseDetail();else if(filterPanel.activeSelf)CloseFilter();else Close();}
#endif
        }
    }
}
