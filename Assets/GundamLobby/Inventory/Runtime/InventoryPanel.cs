using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
namespace GundamLobby
{
 [Serializable] public class InventoryItemData
 {
  public string id,itemName,category,rarity,description;
  public Sprite icon;public bool locked,empty;
  public int quantity=1,price=1600,sellPrice=64;
  public Color rarityColor=Color.cyan;
 }
 public class InventoryPanel:MonoBehaviour
 {
  [Header("Inventory and replaceable assets")]
  public InventoryItemData[] items;
  public InventoryItemTile tilePrefab;
  public GameObject modelPrefab;
  public int selectedIndex,capacity=10,displayedOwned;
  [Header("UI")]
  public GameObject inventoryCanvas,lobbyCanvas,previewRig;
  public Transform content,modelMount;
  public Camera previewCamera;public ScrollRect scroll;
  public TMP_Text itemNameLabel,rarityLabel,descriptionLabel,priceLabel,sellLabel,capacityLabel,toastLabel,sortLabel;
  public CanvasGroup toast;public Button[] categoryButtons;
  public string[] categories={"ALL","MODS","PAINT","PATTERNS","WEAPON","VEHICLE","DEVICES"};
  public HomeLobbyController lobby;public MechaInventoryPresentation presentation;
  readonly List<InventoryItemTile> tiles=new List<InventoryItemTile>();
  string filter="ALL";int sort,progressVersion=-1;float toastUntil;bool lobbyWasEnabled;GameObject model;int screenW,screenH;
  readonly HashSet<string> bulk=new HashSet<string>(); bool bulkMode,bulkDiscard;
  GameObject unlockDialog;TMP_Text unlockMessage;Button unlockConfirm;
  public bool IsOpen=>inventoryCanvas&&inventoryCanvas.activeSelf;
  public int VisibleItemCount=>tiles.Count(t=>t&&t.gameObject.activeSelf);
  public int UnlockedSlotCount=>PlayerProgress.InventorySlots;
  void Awake()
  {
   items=new InventoryItemData[PlayerProgress.Rules.maximumInventorySlots];
   selectedIndex=0;RefreshProgression();
   if(modelPrefab&&modelMount){model=Instantiate(modelPrefab,modelMount);model.name="Inventory Gundam Preview";model.transform.localPosition=Vector3.zero;model.transform.localRotation=Quaternion.identity;var skin=model.AddComponent<GundamSkinBinding>();skin.dashboard=FindFirstObjectByType<HomeLobbyDashboard>();foreach(var t in model.GetComponentsInChildren<Transform>(true))t.gameObject.layer=30;}
   if(previewRig)previewRig.SetActive(false);if(inventoryCanvas)inventoryCanvas.SetActive(false);
   categories=new[]{"ALL","MATERIALS","MODULES","SUPPLIES"};
   for(int i=0;i<categoryButtons.Length;i++)categoryButtons[i].gameObject.SetActive(i<categories.Length);
   categoryButtons=categoryButtons.Take(categories.Length).ToArray();
   for(int i=0;i<categoryButtons.Length;i++){int index=i;categoryButtons[i].GetComponentInChildren<TMP_Text>().text=categories[i];categoryButtons[i].onClick.AddListener(()=>SetCategory(categories[index]));}
  }
  void RefreshProgression()
  {
   string selected=items!=null&&selectedIndex>=0&&selectedIndex<items.Length&&items[selectedIndex]!=null?items[selectedIndex].id:"fcore";
   progressVersion=PlayerProgress.Version;capacity=PlayerProgress.InventorySlots;
   var definitions=GameItems.All.Where(t=>t.id=="fcore"||PlayerProgress.Count(t.id)>0).ToArray();
   for(int i=0;i<items.Length;i++)
   {
    if(i<definitions.Length){var t=definitions[i];items[i]=new InventoryItemData{id=t.id,itemName=t.name,category=t.category,rarity=t.rarity,description=t.description,quantity=PlayerProgress.Count(t.id),price=t.price,sellPrice=t.sell,rarityColor=t.color,icon=MechaItemVisuals.Icon(t.id)};}
    else items[i]=new InventoryItemData{id="empty-"+i,itemName="EMPTY SLOT",category="ALL",empty=true,quantity=0};
    items[i].locked=i>=capacity;
   }
   selectedIndex=Mathf.Max(0,Array.FindIndex(items,t=>t.id==selected));
   displayedOwned=PlayerProgress.UsedSlots;capacityLabel.text=displayedOwned+" / "+capacity;
   foreach(var tile in tiles)tile.Bind(this,tile.dataIndex,items[tile.dataIndex]);
   if(tiles.Count>0){SetCategory(filter);if(tiles.Any(t=>t.gameObject.activeSelf&&t.dataIndex==selectedIndex))Select(selectedIndex);}
  }

  public void SetPreviewModel(GameObject prefab)
  {
   if(!prefab||!modelMount)return;modelPrefab=prefab;if(model){model.SetActive(false);Destroy(model);}
   model=Instantiate(prefab,modelMount);model.name="Inventory Gundam Preview";model.transform.localPosition=Vector3.zero;model.transform.localRotation=Quaternion.identity;
   var skin=model.AddComponent<GundamSkinBinding>();skin.dashboard=FindFirstObjectByType<HomeLobbyDashboard>();foreach(var t in model.GetComponentsInChildren<Transform>(true))t.gameObject.layer=30;
  }
  public void Open()
  {
   if(IsOpen)return;lobbyWasEnabled=lobby&&lobby.enabled;if(lobby)lobby.enabled=false;
   inventoryCanvas.SetActive(true);FitScreen();previewRig.SetActive(true);if(lobbyCanvas)lobbyCanvas.SetActive(false);
   if(tiles.Count==0)for(int i=0;i<items.Length;i++){var t=Instantiate(tilePrefab,content);t.name="Inventory Slot "+(i+1);t.gameObject.SetActive(true);t.Bind(this,i,items[i]);tiles.Add(t);}
   filter="ALL";RefreshProgression();
   Canvas.ForceUpdateCanvases();if(previewCamera.targetTexture&&!previewCamera.targetTexture.IsCreated())previewCamera.targetTexture.Create();if(previewCamera.enabled)previewCamera.Render();
  }
  public void Close()
  {
   if(!IsOpen)return;if(unlockDialog)unlockDialog.SetActive(false);inventoryCanvas.SetActive(false);previewRig.SetActive(false);if(lobbyCanvas)lobbyCanvas.SetActive(true);if(lobby)lobby.enabled=lobbyWasEnabled;
  }
  public void SetCategory(string value)
  {
   filter=value;foreach(var t in tiles){var item=items[t.dataIndex];var definition=GameItems.Find(item.id);bool match=value=="ALL"||definition!=null&&!item.locked&&!item.empty&&((value=="MATERIALS"&&definition.effect==ItemEffect.Upgrade)||(value=="MODULES"&&GameItems.IsEquipment(definition))||(value=="SUPPLIES"&&GameItems.IsSupply(definition)));t.gameObject.SetActive(match);}
   for(int i=0;i<categoryButtons.Length;i++)
   {
    bool active=categories[i]==value;var s=categoryButtons[i].GetComponent<LobbyShape>();s.color=active?new Color(.06f,.20f,.29f):Color.clear;s.border=active?new Color(.125f,.784f,.965f):Color.clear;s.SetVerticesDirty();
    var txt=categoryButtons[i].GetComponentInChildren<TMP_Text>();if(txt)txt.color=active?Color.white:new Color(.51f,.62f,.67f);
   }
   if(scroll)scroll.verticalNormalizedPosition=1;
   if(!tiles.Any(t=>t.gameObject.activeSelf&&t.dataIndex==selectedIndex)){var first=tiles.FirstOrDefault(t=>t.gameObject.activeSelf&&!items[t.dataIndex].locked);if(first)Select(first.dataIndex);else{itemNameLabel.text="NO ITEMS";rarityLabel.text="STORAGE";descriptionLabel.text="Acquire items from campaign rewards or the Shop.";priceLabel.text=sellLabel.text="";var use=FindAction("Use Selected Item");var sell=FindAction("List on Marketplace");if(use)use.interactable=false;if(sell)sell.interactable=false;if(presentation)presentation.Empty();}}
   LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)content);
  }
  public void OnTileClick(int index)
  {
   if(items[index].locked){RequestUnlockSlot();return;}
   if(bulkMode){string id=items[index].id;if(!PlayerProgress.CanSell(id)){Notify("Select tradable, unequipped items. F-Cores are protected.");return;}if(!bulk.Add(id))bulk.Remove(id);foreach(var t in tiles)t.SetSelected(bulk.Contains(items[t.dataIndex].id));UpdateActions();return;}
   Select(index);
  }
  public void Select(int index)
  {
   if(index<0||index>=items.Length||items[index].locked)return;selectedIndex=index;var item=items[index];
   itemNameLabel.text=item.itemName;rarityLabel.text=item.empty?"AVAILABLE STORAGE":item.rarity;
   descriptionLabel.textWrappingMode=TextWrappingModes.Normal;descriptionLabel.overflowMode=TextOverflowModes.Ellipsis;descriptionLabel.rectTransform.sizeDelta=new Vector2(470,80);
   descriptionLabel.text=item.empty?"This unlocked slot is ready for loot.":item.description;
   priceLabel.text=item.empty?"":"OWNED  "+item.quantity.ToString("N0")+"   /   "+(PlayerProgress.IsArmed(item.id)?"ARMED FOR NEXT SORTIE":PlayerProgress.IsEquipped(item.id)?"EQUIPPED":"IN STORAGE");
   sellLabel.text=item.empty?"":item.sellPrice>0?"SELL VALUE  "+item.sellPrice.ToString("N0")+" CREDITS EACH":"PROTECTED UPGRADE MATERIAL";
   foreach(var t in tiles)t.SetSelected(t.dataIndex==index);UpdateActions();if(presentation)presentation.Show(item);
  }
  public void SortItems()
  {
   sort=(sort+1)%3;IEnumerable<InventoryItemTile> list=tiles;
   if(sort==1)list=list.OrderBy(t=>items[t.dataIndex].itemName);else if(sort==2)list=list.OrderByDescending(t=>items[t.dataIndex].quantity);else list=list.OrderBy(t=>t.dataIndex);
   list=list.OrderBy(t=>items[t.dataIndex].locked?2:items[t.dataIndex].empty?1:0);
   foreach(var t in list)t.transform.SetAsLastSibling();if(sortLabel)sortLabel.text=new[]{"TYPE","NAME","QUANTITY"}[sort];if(scroll)scroll.verticalNormalizedPosition=1;
  }
  Button FindAction(string name)=>inventoryCanvas.GetComponentsInChildren<Button>(true).FirstOrDefault(b=>b.name==name);
  void UpdateActions()
  {
   var item=GameItems.Find(items[selectedIndex].id);var use=FindAction("Use Selected Item");var sell=FindAction("List on Marketplace");
   if(use){use.interactable=bulkMode||item!=null&&(item.effect==ItemEffect.Upgrade||PlayerProgress.Count(item.id)>0);use.GetComponentInChildren<TMP_Text>().text=bulkMode?"CANCEL SELECTION":item==null?"EMPTY SLOT":PlayerProgress.PreparedItem(FindFirstObjectByType<HomeLobbyDashboard>().ActiveUnit.id)==item.id?"CANCEL PREPARATION":item.Action;}
   if(sell){sell.interactable=!bulkMode&&item!=null&&PlayerProgress.CanSell(item.id);sell.GetComponentInChildren<TMP_Text>().text="SELL SELECTED";}
   var b=FindAction("BULK SELL");if(b)b.GetComponentInChildren<TMP_Text>().text=bulkMode&&!bulkDiscard?"SELL ("+bulk.Count+")":"BULK SELL";
   b=FindAction("BULK DISCARD");if(b)b.GetComponentInChildren<TMP_Text>().text=bulkMode&&bulkDiscard?"DISCARD ("+bulk.Count+")":"BULK DISCARD";
  }
  public void UseSelected()
  {
   if(bulkMode){bulkMode=false;bulk.Clear();Select(selectedIndex);return;}
   if(!tiles.Any(t=>t.gameObject.activeSelf&&t.dataIndex==selectedIndex))return;var item=GameItems.Find(items[selectedIndex].id);if(item==null)return;var d=FindFirstObjectByType<HomeLobbyDashboard>();
   if(item.effect==ItemEffect.Upgrade){LobbyServices.For(d).Open("UPDATE");return;}
   if(GameItems.IsEquipment(item)){Close();d.OpenWeaponry();d.weaponry.Loadout.FocusItem(item.id);return;}
   PlayerProgress.UseItem(item.id,d.ActiveUnit.id,out var message);Notify(message);RefreshProgression();
  }
  public void SellSelected(){if(!PlayerProgress.CanSell(items[selectedIndex].id))return;ConfirmStacks(new[]{items[selectedIndex].id},false);}
  public void BeginBulkSell(){BeginBulk(false);}
  public void BeginBulkDiscard(){BeginBulk(true);}
  void BeginBulk(bool discard)
  {
   if(bulkMode&&bulkDiscard==discard){if(bulk.Count==0){Notify("Select item stacks first.");return;}ConfirmStacks(bulk.ToArray(),discard);return;}
   bulkMode=true;bulkDiscard=discard;bulk.Clear();foreach(var t in tiles)t.SetSelected(false);UpdateActions();Notify("Select item stacks, then press "+(discard?"DISCARD":"SELL")+" again. USE cancels selection.");
  }
  void ConfirmStacks(string[] ids,bool discard)
  {
   int total=ids.Sum(id=>(GameItems.Find(id)?.sell??0)*PlayerProgress.Count(id));
   string names=string.Join("\n",ids.Select(id=>PlayerProgress.Count(id)+" x "+GameItems.Find(id).name));
   LobbyServices.For(FindFirstObjectByType<HomeLobbyDashboard>()).Confirm(discard?"DISCARD ITEM STACKS":"SELL ITEM STACKS",names+"\n\n"+(discard?"These items will be removed without a refund.":"Receive "+total.ToString("N0")+" Credits. This sells the full selected stacks."),()=>{PlayerProgress.SellItems(ids,discard,out var msg);bulkMode=false;bulk.Clear();RefreshProgression();Notify(msg);});
  }
  public void RequestUnlockSlot()
  {
   if(PlayerProgress.InventorySlots>=PlayerProgress.Rules.maximumInventorySlots){Notify("All slots are unlocked.");return;}
   if(!unlockDialog)
   {
    var root=ProgressionUi.Shape("Unlock Storage Confirmation",inventoryCanvas.transform,0,0,1920,1080,new Color(0,.02f,.035f,.84f),Color.clear);root.raycastTarget=true;unlockDialog=root.gameObject;
    var box=ProgressionUi.Shape("Dialog",root.transform,630,345,660,350,new Color(.025f,.065f,.10f,.99f),ProgressionUi.Cyan);
    ProgressionUi.Text("Title","UNLOCK STORAGE SLOT",box.transform,itemNameLabel.font,30,28,600,53,32,Color.white);
    unlockMessage=ProgressionUi.Text("Price","",box.transform,itemNameLabel.font,30,106,600,100,26,ProgressionUi.Muted);unlockMessage.textWrappingMode=TextWrappingModes.Normal;
    unlockConfirm=ProgressionUi.Button("Confirm Unlock","UNLOCK",box.transform,itemNameLabel.font,30,250,285,61,ConfirmUnlock,true);
    ProgressionUi.Button("Cancel Unlock","CANCEL",box.transform,itemNameLabel.font,345,250,285,61,()=>unlockDialog.SetActive(false));
   }
   unlockDialog.transform.SetAsLastSibling();unlockDialog.SetActive(true);
   unlockMessage.text="SLOT "+(PlayerProgress.InventorySlots+1)+" / "+PlayerProgress.Rules.maximumInventorySlots+"\nCOST "+PlayerProgress.NextSlotPrice.ToString("N0")+" CREDITS   ·   BALANCE "+PlayerProgress.Credits.ToString("N0");
   unlockConfirm.interactable=PlayerProgress.Credits>=PlayerProgress.NextSlotPrice;
  }
  public void ConfirmUnlock()
  {
   PlayerProgress.ExpandInventory(out string message);Notify(message);if(unlockDialog)unlockDialog.SetActive(false);RefreshProgression();
  }
  public void Notify(string message){toastLabel.text=message;toastUntil=Time.unscaledTime+2.8f;}
  void FitScreen(){screenW=Screen.width;screenH=Screen.height;var scaler=inventoryCanvas.GetComponent<CanvasScaler>();if(scaler)scaler.matchWidthOrHeight=screenW/(float)Mathf.Max(1,screenH)>=16f/9f?1:0;}
  void Update()
  {
   if(!IsOpen)return;if(LobbyServices.BlocksInput)return;if(progressVersion!=PlayerProgress.Version)RefreshProgression();if(screenW!=Screen.width||screenH!=Screen.height)FitScreen();
   if(toast)toast.alpha=Mathf.MoveTowards(toast.alpha,Time.unscaledTime<toastUntil?1:0,Time.unscaledDeltaTime*6);
   if(unlockDialog&&unlockDialog.activeSelf){if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame)unlockDialog.SetActive(false);return;}
   if(Keyboard.current!=null){if(Keyboard.current.escapeKey.wasPressedThisFrame)Close();else if(Keyboard.current.spaceKey.wasPressedThisFrame)UseSelected();}
  }
 }
}
