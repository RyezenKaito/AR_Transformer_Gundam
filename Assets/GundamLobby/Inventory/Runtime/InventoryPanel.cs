using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace GundamLobby
{
 [Serializable] public class InventoryItemData
 {
  public string itemName,category,rarity,description;
  public Sprite icon;
  public int quantity=1,price=1600,sellPrice=64;
  public Color rarityColor=Color.cyan;
 }
 public class InventoryPanel:MonoBehaviour
 {
  [Header("Inventory and replaceable assets")]
  public InventoryItemData[] items;
  public InventoryItemTile tilePrefab;
  public GameObject modelPrefab;
  public int selectedIndex=3,capacity=240,displayedOwned=113;
  [Header("UI")]
  public GameObject inventoryCanvas,lobbyCanvas,previewRig;
  public Transform content,modelMount;
  public Camera previewCamera;
  public ScrollRect scroll;
  public TMP_Text itemNameLabel,rarityLabel,descriptionLabel,priceLabel,sellLabel,capacityLabel,toastLabel,sortLabel;
  public CanvasGroup toast;
  public Button[] categoryButtons;
  public string[] categories={"ALL","MODS","PAINT","PATTERNS","WEAPON","VEHICLE","DEVICES"};
  public HomeLobbyController lobby;
  readonly List<InventoryItemTile> tiles=new List<InventoryItemTile>();
  string filter="ALL";int sort;float toastUntil;bool lobbyWasEnabled;GameObject model;int screenW,screenH;
  public bool IsOpen=>inventoryCanvas&&inventoryCanvas.activeSelf;
  public int VisibleItemCount=>tiles.Count(t=>t&&t.gameObject.activeSelf);
  void Awake()
  {
   if(modelPrefab&&modelMount){model=Instantiate(modelPrefab,modelMount);model.name="Inventory Gundam Preview";model.transform.localPosition=Vector3.zero;model.transform.localRotation=Quaternion.identity;foreach(var t in model.GetComponentsInChildren<Transform>(true))t.gameObject.layer=30;}
   if(previewRig)previewRig.SetActive(false);if(inventoryCanvas)inventoryCanvas.SetActive(false);
   for(int i=0;categoryButtons!=null&&i<categoryButtons.Length;i++){int index=i;categoryButtons[i].onClick.AddListener(()=>SetCategory(categories[index]));}
  }
  public void SetPreviewModel(GameObject prefab)
  {
   if(!prefab||!modelMount)return;
   modelPrefab=prefab;if(model){model.SetActive(false);Destroy(model);}
   model=Instantiate(prefab,modelMount);model.name="Inventory Gundam Preview";model.transform.localPosition=Vector3.zero;model.transform.localRotation=Quaternion.identity;
   foreach(var t in model.GetComponentsInChildren<Transform>(true))t.gameObject.layer=30;
  }
  public void Open()
  {
   if(IsOpen)return;
   lobbyWasEnabled=lobby&&lobby.enabled;if(lobby)lobby.enabled=false;
   inventoryCanvas.SetActive(true);FitScreen();previewRig.SetActive(true);if(lobbyCanvas)lobbyCanvas.SetActive(false);
   if(tiles.Count==0)for(int i=0;i<items.Length;i++){var t=Instantiate(tilePrefab,content);t.name="Inventory Item "+i;t.gameObject.SetActive(true);t.Bind(this,i,items[i]);tiles.Add(t);}
   capacityLabel.text=displayedOwned+"/"+capacity;SetCategory("ALL");Select(Mathf.Clamp(selectedIndex,0,items.Length-1));
   Canvas.ForceUpdateCanvases();previewCamera.enabled=true;if(previewCamera.targetTexture&&!previewCamera.targetTexture.IsCreated())previewCamera.targetTexture.Create();previewCamera.Render();
  }
  public void Close()
  {
   if(!IsOpen)return;inventoryCanvas.SetActive(false);previewRig.SetActive(false);if(lobbyCanvas)lobbyCanvas.SetActive(true);if(lobby)lobby.enabled=lobbyWasEnabled;
  }
  public void SetCategory(string value)
  {
   filter=value;foreach(var t in tiles)t.gameObject.SetActive(value=="ALL"||items[t.dataIndex].category==value);
   for(int i=0;i<categoryButtons.Length;i++)
   {
    bool active=categories[i]==value;var s=categoryButtons[i].GetComponent<LobbyShape>();s.color=active?new Color(.06f,.20f,.29f):Color.clear;s.border=active?new Color(.125f,.784f,.965f):Color.clear;s.SetVerticesDirty();
    var txt=categoryButtons[i].GetComponentInChildren<TMP_Text>();if(txt)txt.color=active?Color.white:new Color(.51f,.62f,.67f);
   }
   if(scroll)scroll.verticalNormalizedPosition=1;
   if(!tiles.Any(t=>t.gameObject.activeSelf&&t.dataIndex==selectedIndex)){var first=tiles.FirstOrDefault(t=>t.gameObject.activeSelf);if(first)Select(first.dataIndex);}
   LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)content);
  }
  public void Select(int index)
  {
   if(index<0||index>=items.Length)return;selectedIndex=index;var item=items[index];
   itemNameLabel.text=item.itemName;rarityLabel.text=item.rarity;descriptionLabel.text=item.description;priceLabel.text="Price:  "+item.price.ToString("N0");sellLabel.text="Min. selling price:  "+item.sellPrice.ToString("N0");
   foreach(var t in tiles)t.SetSelected(t.dataIndex==index);
  }
  public void SortItems()
  {
   sort=(sort+1)%3;IEnumerable<InventoryItemTile> list=tiles;
   if(sort==1)list=list.OrderBy(t=>items[t.dataIndex].itemName);else if(sort==2)list=list.OrderByDescending(t=>items[t.dataIndex].quantity);else list=list.OrderBy(t=>t.dataIndex);
   foreach(var t in list)t.transform.SetAsLastSibling();if(sortLabel)sortLabel.text=new[]{"TYPE","NAME","QUANTITY"}[sort];
   if(scroll)scroll.verticalNormalizedPosition=1;
  }
  public void UseSelected(){if(items.Length>0)Notify("USE  /  "+items[selectedIndex].itemName+"  ·  Prototype feedback");}
  public void Notify(string message){toastLabel.text=message;toastUntil=Time.unscaledTime+2.8f;}
  void FitScreen(){screenW=Screen.width;screenH=Screen.height;var scaler=inventoryCanvas.GetComponent<CanvasScaler>();if(scaler)scaler.matchWidthOrHeight=screenW/(float)Mathf.Max(1,screenH)>=16f/9f?1:0;}
  void Update()
  {
   if(!IsOpen)return;if(screenW!=Screen.width||screenH!=Screen.height)FitScreen();
   if(toast)toast.alpha=Mathf.MoveTowards(toast.alpha,Time.unscaledTime<toastUntil?1:0,Time.unscaledDeltaTime*6);
#if ENABLE_INPUT_SYSTEM
   if(Keyboard.current!=null){if(Keyboard.current.escapeKey.wasPressedThisFrame)Close();else if(Keyboard.current.spaceKey.wasPressedThisFrame)UseSelected();}
#elif ENABLE_LEGACY_INPUT_MANAGER
   if(Input.GetKeyDown(KeyCode.Escape))Close();else if(Input.GetKeyDown(KeyCode.Space))UseSelected();
#endif
  }
 }
}
