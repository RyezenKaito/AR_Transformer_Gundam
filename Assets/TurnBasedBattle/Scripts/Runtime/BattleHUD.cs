using UnityEngine;
using UnityEngine.UI;

namespace TurnBasedBattle
{
    [DefaultExecutionOrder(100)]
    public sealed class BattleHUD : MonoBehaviour
    {
        public TurnBattleManager battle;
        public Text phaseText, roundText, messageText, playerHPText, ultimateText, actionText, selectionText, confirmText;
        public Text cueTitle, cueHint, debugText, speedText, resultTitle;
        public Image playerHPFill, ultimateFill, actionProgress, cueProgress, cueWindow, cueCursor;
        public Button attackButton, skillButton, guardButton, ultimateButton, confirmButton;
        public Button[] enemyButtons;
        public Text[] enemyNames, enemyHPTexts;
        public Image[] enemyHPFills, enemyBorders;
        public GameObject cuePanel, commandPanel, debugPanel, pausePanel, resultPanel;
        public Text[] floatingTexts;
        public RectTransform floatingRoot;
        public RectTransform cueTrack;
        float[] floatAge;
        Vector2[] floatStart;
        BattleActor[] floatActors;
        int floatCursor;
        string lastSummary="";
        Sprite fillSprite;
        Color cyan=new Color(.2f,.88f,1),gold=new Color(1,.79f,.25f),muted=new Color(.34f,.47f,.56f);

        void Awake()
        {
            floatAge=new float[floatingTexts.Length];floatStart=new Vector2[floatingTexts.Length];floatActors=new BattleActor[floatingTexts.Length];for(int i=0;i<floatAge.Length;i++)floatAge[i]=100;
            // Filled Images need a sprite; an untextured Image always renders a full rectangle.
            fillSprite=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),new Vector2(.5f,.5f));
            foreach(var image in GetComponentsInChildren<Image>(true))if(image.type==Image.Type.Filled)image.sprite=fillSprite;
            // Bind in play mode: editor-created lambda listeners do not survive scene serialization.
            Bind("Attack",()=>battle.Select(BattleActionKind.Attack));
            Bind("Skill",()=>battle.Select(BattleActionKind.Skill));
            Bind("Guard",()=>battle.Select(BattleActionKind.Guard));
            Bind("Ultimate",()=>battle.Select(BattleActionKind.EarthPrison));
            Bind("Confirm",()=>battle.Confirm());
            Bind("Debug",ToggleDebug);Bind("Pause",()=>battle.TogglePause());Bind("Resume",()=>battle.TogglePause());
            Bind("FillUlt",()=>battle.FillUltimate());Bind("OneEnemy",()=>battle.Restart(1));Bind("ThreeEnemies",()=>battle.Restart(3));
            Bind("HalfSpeed",()=>battle.SetPlaybackSpeed(.5f));Bind("NormalSpeed",()=>battle.SetPlaybackSpeed(1));Bind("DoubleSpeed",()=>battle.SetPlaybackSpeed(2));
            Bind("EasyDodge",()=>battle.ToggleEasy());Bind("Retry",()=>battle.Restart(1));Bind("Retry3",()=>battle.Restart(3));
            Bind("TouchDodge",()=>battle.DodgeNow());
            for(int i=0;i<enemyButtons.Length;i++){int index=i;enemyButtons[i].onClick.RemoveAllListeners();enemyButtons[i].onClick.AddListener(()=>battle.SelectTarget(index));}
        }
        void Bind(string name,UnityEngine.Events.UnityAction action)
        {
            foreach(var button in GetComponentsInChildren<Button>(true))if(button.name==name){button.onClick.RemoveAllListeners();button.onClick.AddListener(action);return;}
        }
        void OnDestroy(){if(fillSprite!=null)Destroy(fillSprite);}
        public void ClearFloating()
        {
            if(floatAge==null)return;
            for(int i=0;i<floatAge.Length;i++){floatAge[i]=100;floatingTexts[i].gameObject.SetActive(false);}
        }
        void LateUpdate()
        {
            PositionEnemyBars();
            if(floatAge==null)return;
            for(int i=0;i<floatAge.Length;i++)
            {
                if(floatAge[i]>=1.2f){floatingTexts[i].gameObject.SetActive(false);continue;}
                if(battle==null || !battle.Paused)floatAge[i]+=Time.unscaledDeltaTime;
                if(floatActors[i] && battle && battle.battleCamera)
                {
                    Vector3 screen=battle.battleCamera.WorldToScreenPoint(floatActors[i].HealthBarPoint);
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(floatingRoot,screen,null,out floatStart[i]);
                    floatingTexts[i].gameObject.SetActive(screen.z>0);
                }
                floatingTexts[i].rectTransform.anchoredPosition=floatStart[i]+Vector2.up*floatAge[i]*55;
                var color=floatingTexts[i].color;color.a=1-Mathf.InverseLerp(.65f,1.2f,floatAge[i]);floatingTexts[i].color=color;
            }
        }
        void PositionEnemyBars()
        {
            if(!battle || !battle.battleCamera)return;
            for(int i=0;i<enemyButtons.Length;i++)
            {
                bool exists=i<battle.enemies.Length && battle.enemies[i] && battle.enemies[i].gameObject.activeSelf;
                if(!exists){enemyButtons[i].gameObject.SetActive(false);continue;}
                Vector3 screen=battle.battleCamera.WorldToScreenPoint(battle.enemies[i].HealthBarPoint);
                bool visible=screen.z>battle.battleCamera.nearClipPlane && screen.x>=0 && screen.x<=Screen.width && screen.y>=0 && screen.y<=Screen.height;
                enemyButtons[i].gameObject.SetActive(visible);if(!visible)continue;
                var card=(RectTransform)enemyButtons[i].transform;
                RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)card.parent,screen,null,out Vector2 local);
                card.anchorMin=card.anchorMax=new Vector2(.5f,.5f);card.pivot=new Vector2(.5f,0);
                card.sizeDelta=new Vector2(220,68);card.anchoredPosition=local;
            }
        }
        public void Refresh(bool force=false)
        {
            if(battle==null || battle.player==null)return;
            var b=battle;
            playerHPFill.fillAmount=b.player.HP/Mathf.Max(1,b.player.maxHP);
            ultimateFill.fillAmount=b.Ultimate/100;
            for(int i=0;i<enemyButtons.Length;i++)
            {
                bool active=i<b.enemies.Length && b.enemies[i].gameObject.activeSelf;
                enemyButtons[i].gameObject.SetActive(active);if(!active)continue;
                var e=b.enemies[i];enemyHPFills[i].fillAmount=e.HP/Mathf.Max(1,e.maxHP);
                enemyButtons[i].interactable=b.CommandsAvailable && !e.Dead;
                enemyBorders[i].color=e==b.SelectedTarget && b.CommandsAvailable?gold:new Color(.16f,.27f,.34f,.8f);
            }
            attackButton.interactable=b.CanSelect(BattleActionKind.Attack);
            skillButton.interactable=b.CanSelect(BattleActionKind.Skill);
            guardButton.interactable=b.CanSelect(BattleActionKind.Guard);
            ultimateButton.interactable=b.CanSelect(BattleActionKind.EarthPrison);
            confirmButton.gameObject.SetActive(b.Phase==BattlePhase.PlayerTargeting && !b.Paused);
            pausePanel.SetActive(b.Paused);resultPanel.SetActive(b.Phase==BattlePhase.Victory || b.Phase==BattlePhase.Defeat);
            actionProgress.fillAmount=b.Current!=null?b.ActionTime/b.Current.duration:0;

            int cue=b.CueIndex;cuePanel.SetActive(cue>=0 && !b.Paused);
            if(cue>=0)
            {
                var hit=b.Current.hits[cue];var attempt=b.GetAttempt(cue);
                float span=hit.time-hit.cue;
                float p=Mathf.Clamp01((b.ActionTime-hit.cue)/span);
                cueProgress.fillAmount=p;
                var window=cueWindow.rectTransform;
                float extra=b.EasyDodge?.09f:b.tuning.extraDodgeWindow;
                window.anchorMin=new Vector2(Mathf.Clamp01((hit.open-extra-hit.cue)/span),0);
                window.anchorMax=new Vector2(Mathf.Clamp01((hit.close+extra-hit.cue)/span),1);window.offsetMin=window.offsetMax=Vector2.zero;
                var cursor=cueCursor.rectTransform;cursor.anchorMin=cursor.anchorMax=new Vector2(p,.5f);cursor.anchoredPosition=Vector2.zero;
                bool success=attempt!=null && attempt.outcome==DodgeOutcome.Dodged;
                bool failed=attempt!=null && attempt.pressed && !success;
                cueTitle.text=success?"NÉ THÀNH CÔNG":failed?"LỆCH NHỊP":"SPACE  ·  NÉ ĐÚNG NHỊP";
                cueTitle.color=success?cyan:failed?new Color(1,.35f,.25f):Color.white;
                cueHint.text=success?"Giữ vị trí · đòn đánh đi sượt qua":failed?"Mỗi hit chỉ nhận một lần bấm":"Bấm một lần khi vạch vào vùng vàng";
            }
            string summary=$"{b.Phase}|{b.PlayerTurn}|{b.Ultimate}|{b.player.HP}|{b.SelectedAction}|{b.SelectedTarget?.actorId}|{b.SkillWait}|{b.Message}|{b.Guarding}|{b.Paused}|{b.PlaybackSpeed}|{b.EasyDodge}|{b.HitRecords.Count}";
            if(!force && summary==lastSummary)return;lastSummary=summary;
            phaseText.text=b.Phase==BattlePhase.PlayerCommand || b.Phase==BattlePhase.PlayerTargeting?"LƯỢT CỦA BẠN":b.Phase==BattlePhase.PlayerAction?"ĐANG THI TRIỂN":b.Phase==BattlePhase.EnemyAction?"QUÁI ĐANG TẤN CÔNG":b.Phase==BattlePhase.Victory?"CHIẾN THẮNG":"THẤT BẠI";
            phaseText.color=b.Phase==BattlePhase.EnemyAction?new Color(1,.48f,.32f):cyan;
            roundText.text=$"ROUND {b.PlayerTurn:00}  /  {b.initialEnemies} SENTINEL";
            messageText.text=b.Message;
            messageText.rectTransform.anchorMin=new Vector2(.025f,.18f);
            messageText.rectTransform.anchorMax=new Vector2(.51f,.235f);
            playerHPText.text=$"{Mathf.CeilToInt(b.player.HP)} / {Mathf.CeilToInt(b.player.maxHP)}";
            ultimateText.text=b.Ultimate>=100?"ULTIMATE READY":$"ULTIMATE  {b.Ultimate:0} / 100";
            ultimateText.color=b.Ultimate>=100?gold:muted;
            actionText.text=b.Current!=null?b.Current.displayName:b.Guarding?"PHÒNG THỦ ĐANG BẬT":"CHỌN HÀNH ĐỘNG";
            selectionText.text=b.Phase==BattlePhase.PlayerTargeting?(b.SelectedAction==BattleActionKind.EarthPrison?"MỤC TIÊU: TOÀN BỘ QUÁI":b.SelectedAction==BattleActionKind.Guard?"GIẢM 50% DAMAGE PHA QUÁI KẾ TIẾP":"MỤC TIÊU: "+b.SelectedTarget?.actorName):"1 Tấn công   2 Kỹ năng   3 Phòng thủ   R Ultimate";
            confirmText.text=b.SelectedAction==BattleActionKind.EarthPrison?"THI TRIỂN ULTIMATE  ↵":"XÁC NHẬN  ↵";
            for(int i=0;i<enemyButtons.Length && i<b.enemies.Length;i++)
            {var e=b.enemies[i];enemyNames[i].text=e.actorName+(e.Dead?" · ĐÃ HẠ":"");enemyHPTexts[i].text=$"{Mathf.CeilToInt(e.HP)} / {e.maxHP:0}";}
            var skillLabel=skillButton.GetComponentInChildren<Text>();if(skillLabel!=null)skillLabel.text=b.SkillWait>0?$"2  KỸ NĂNG\nCòn {b.SkillWait} lượt":"2  KỸ NĂNG\nPháo năng lượng";
            speedText.text=$"TỐC ĐỘ {b.PlaybackSpeed:0.0}× · NÉ {(b.EasyDodge?"DỄ":"CHUẨN")}";
            debugText.text=$"F2: nạp Ultimate   F5: 1v1   F6: 1v3\nP: tạm dừng   TAB: đổi mục tiêu\nLượt {b.PlayerTurn} · Action {b.ActionId} · {b.HitRecords.Count} hit records";
            resultTitle.text=b.Phase==BattlePhase.Victory?"CHIẾN THẮNG":"THẤT BẠI";
        }
        public void ToggleDebug(){debugPanel.SetActive(!debugPanel.activeSelf);Refresh(true);}
        public void ShowResult(BattleActor actor,string value,bool dodge)
        {
            if(floatAge==null || floatingTexts.Length==0 || battle.battleCamera==null)return;
            int i=floatCursor++%floatingTexts.Length;
            floatActors[i]=actor;
            Vector3 screen=battle.battleCamera.WorldToScreenPoint(actor.HealthBarPoint);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(floatingRoot,screen,null,out Vector2 local);
            floatStart[i]=local;floatAge[i]=0;
            floatingTexts[i].text=value;floatingTexts[i].color=dodge?cyan:actor.player?new Color(1,.38f,.27f):gold;
            floatingTexts[i].gameObject.SetActive(true);
        }
    }
}
