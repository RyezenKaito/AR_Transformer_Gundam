from pathlib import Path
p=Path('Assets/TurnBasedBattle/Scripts/Editor')
f=p/'TurnBattleBuilder.cs'
s=f.read_text(encoding='utf-8')
s=s.replace('BuildPlayer();','BuildPlayer(manager.tuning);').replace('BuildEnemy();','BuildEnemy(manager.tuning);').replace('static BattleActor BuildPlayer()','static BattleActor BuildPlayer(BattleTuning tuning)').replace('static BattleActor BuildEnemy()','static BattleActor BuildEnemy(BattleTuning tuning)')
s=s.replace('Quaternion.Euler(0,110,0)','Quaternion.Euler(0,90,0)').replace('Quaternion.Euler(0,-110,0)','Quaternion.Euler(0,-90,0)')
s=s.replace('Root+"/Animations/Player");','Root+"/Animations/Player",tuning);').replace('Root+"/Animations/Enemy");','Root+"/Animations/Enemy",tuning);')
start=s.index('            var gunMat=Lit("PlayerWeapon"')
end=s.index('            actor.bodyRenderers=',start)
s=s[:start]+'''            BattleWeaponBuilder.Build(actor);
'''+s[end:]
start=s.index('            var gun=Node("ArmCannon"')
end=s.index('            PrefabUtility.SaveAsPrefabAsset(g,Root+"/Prefabs/Sentinel.prefab")',start)
s=s[:start]+'''            var actor=g.AddComponent<BattleActor>();actor.rig=rig;actor.visual=visual.transform;
            actor.clips=BattleClipBuilder.Build(rig,Root+"/Animations/Enemy",tuning);BattleWeaponBuilder.Build(actor);
            actor.bodyRenderers=g.GetComponentsInChildren<Renderer>(true);actor.PreviewPose("IdleBattle",0);
'''+s[end:]
start=s.index('            t.attack=Make(')
end=s.index('            t.enemyCombos=true;',start)
s=s[:start]+'''            void Flight(BattleActionDefinition a,BattleWeaponKind weapon,float stop,bool melee=false)
            {
                a.approachTarget=true;a.weapon=weapon;a.melee=melee;a.stopDistance=stop;
                a.travelStart=.65f;a.arriveTime=1.25f;a.returnStart=2.15f;a.returnEnd=3.02f;a.travelLift=.65f;
                a.gripTime=.42f;a.drawEnd=.66f;a.bladeOn=.72f;a.bladeOff=1.96f;a.sheatheStart=3.08f;a.holsterTime=3.52f;
                EditorUtility.SetDirty(a);
            }
            t.attack=Make("BasicAttack",BattleActionKind.Attack,"TẤN CÔNG · Rút kiếm / áp sát","BasicAttack",3.85f,1.45f,new[]{new BattleHit{time=1.6f,damage=100}},25);
            Flight(t.attack,BattleWeaponKind.Saber,1.55f,true);
            t.skill=Make("EnergyCannon",BattleActionKind.Skill,"KỸ NĂNG · Áp sát / pháo năng lượng","SkillCast",4.0f,1.5f,new[]{new BattleHit{time=1.8f,damage=180}},20,0,2);
            Flight(t.skill,BattleWeaponKind.Cannon,3.1f);
            t.guard=Make("Guard",BattleActionKind.Guard,"PHÒNG THỦ · Giảm 50% sát thương","Guard",.65f,0,Array.Empty<BattleHit>());
            t.ultimate=Make("EarthPrison",BattleActionKind.EarthPrison,"EARTH PRISON · Bay tới / lồng sét","EarthPrisonCast",10.9f,4.2f,new[]{new BattleHit{time=3.35f,damage=120},new BattleHit{time=5.1f,damage=60},new BattleHit{time=5.5f,damage=60},new BattleHit{time=5.9f,damage=60},new BattleHit{time=6.3f,damage=60}},0,100,0,true);
            Flight(t.ultimate,BattleWeaponKind.None,4.85f);t.ultimate.vfxStart=1.55f;t.ultimate.travelStart=.35f;t.ultimate.returnStart=9.55f;t.ultimate.returnEnd=10.6f;
            t.enemyStrike=Make("EnemyStrike",BattleActionKind.EnemyStrike,"SENTINEL · Rút kiếm / phá giáp","EnemyStrike",3.85f,1.45f,new[]{new BattleHit{time=1.6f,damage=80,cue=.7f,open=1.35f,close=1.55f}});
            Flight(t.enemyStrike,BattleWeaponKind.Saber,1.55f,true);
            t.enemyCombo=Make("EnemyCombo",BattleActionKind.EnemyCombo,"SENTINEL · Bay tới / liên xạ","EnemyCombo",4.9f,1.3f,new[]{new BattleHit{time=1.6f,damage=55,cue=.7f,open=1.35f,close=1.55f},new BattleHit{time=2.6f,damage=55,cue=1.85f,open=2.35f,close=2.55f}});
            Flight(t.enemyCombo,BattleWeaponKind.Cannon,3.1f);t.enemyCombo.returnStart=3.05f;t.enemyCombo.returnEnd=3.92f;t.enemyCombo.sheatheStart=4.0f;t.enemyCombo.holsterTime=4.5f;
'''+s[end:]
# A physical floor is used by the camera cast; the visual panels remain collider-free.
anchor='            BattleMeshOptimizer.Combine(stage.transform,"ArenaStatic");'
s=s.replace(anchor,anchor+'''
            var floor=Node("CameraFloor",stage.transform,new Vector3(0,-.3f,0));floor.layer=2;var floorCollider=floor.AddComponent<BoxCollider>();floorCollider.size=new Vector3(40,.5f,32);
''')
f.write_text(s,encoding='utf-8')

f=p/'BattleClipBuilder.cs';s=f.read_text(encoding='utf-8')
s=s.replace('Build(MechaRigData data,string directory)','Build(MechaRigData data,string directory,BattleTuning tuning=null)')
# Replace five action clips using the definitions themselves as the single timing source.
import re
for name,field in [('BasicAttack','attack'),('SkillCast','skill'),('EarthPrisonCast','ultimate'),('EnemyStrike','enemyStrike'),('EnemyCombo','enemyCombo')]:
    s=re.sub(r'            clips.Add\(Clip\(directory,"'+name+r'"[^\n]+',f'            clips.Add(Clip(directory,"{name}",tuning.{field}.duration,false,ActionMarks(tuning.{field},ready)));',s)
s=s.replace('.R("UpperArm_L",-40,0,-12).R("LowerArm_L",-40).R("UpperArm_R",-22,0,14).R("LowerArm_R",-60)', '.R("UpperArm_L",-5,0,-8).R("LowerArm_L",-12).R("UpperArm_R",-5,0,8).R("LowerArm_R",-12)')
s=s.replace('.R("UpperArm_R",-23,0,14)', '.R("UpperArm_R",-6,0,8)').replace('.R("LowerArm_L",-41)', '.R("LowerArm_L",-13)')
index=s.index('        static Mark[] CloseStrike(')
end=s.index('        static Pose Ready()',index)
s=s[:index]+'''        static Mark[] ActionMarks(BattleActionDefinition a,Pose ready)
        {
            var prepare=Brace(.10f).R("Spine",7).R("Chest",3,-5).R("Head",-4,5);
            var draw=Brace(.06f).R("Chest",0,-8).R("UpperArm_R",-35,0,25).R("LowerArm_R",-85);
            if(a.weapon==BattleWeaponKind.Cannon)draw=Brace(.06f).R("Chest",0,8).R("UpperArm_L",-38,0,-22).R("LowerArm_L",-75);
            var boost=Brace(.04f).R("Spine",16).R("Head",-12).R("UpperArm_L",-25,0,-15).R("LowerArm_L",-65).R("UpperArm_R",-30,0,18).R("LowerArm_R",-85).R("UpperLeg_L",-18).R("LowerLeg_L",45).R("Foot_L",-27).R("UpperLeg_R",14).R("LowerLeg_R",32).R("Foot_R",-30);
            var windup=Brace(.07f).R("Spine",3).R("Chest",2,-18).R("Head",-2,14).R("UpperArm_R",-65,-22,28).R("LowerArm_R",-95);
            var aim=Brace(.06f).R("Spine",4).R("UpperArm_L",-53,0,-10).R("LowerArm_L",-27).R("UpperArm_R",-40,-10,8).R("LowerArm_R",-75);
            var returnPose=boost.Copy().R("Spine",12).R("Head",-8);
            var marks=new List<Mark>{K(0,ready),K(.18f,prepare)};
            if(a.weapon!=BattleWeaponKind.None){marks.Add(K(a.gripTime,prepare));marks.Add(K(a.drawEnd,draw));}
            marks.Add(K(a.travelStart+.17f,boost));marks.Add(K(a.arriveTime-.16f,boost));
            if(a.kind==BattleActionKind.EarthPrison)
            {
                var gather=Brace(.055f).R("Chest",-2,-4).R("Head",-8,4).R("UpperArm_L",-74,-8,-18).R("LowerArm_L",-65).R("UpperArm_R",-35,8,22).R("LowerArm_R",-70);
                var orb=gather.Copy().R("UpperArm_L",-82,0,-14).R("Chest",-3,0).R("Head",-10);
                marks.Add(K(a.arriveTime,prepare));marks.Add(K(a.vfxStart,prepare));marks.Add(K(a.vfxStart+.6f,gather));marks.Add(K(a.vfxStart+1.2f,orb));
                marks.Add(K(a.vfxStart+2.45f,orb.Copy().R("Chest",-2,1)));marks.Add(K(a.vfxStart+2.65f,orb));
                marks.Add(K(a.vfxStart+2.88f,aim));marks.Add(K(a.vfxStart+3.2f,aim));
                marks.Add(K(a.vfxStart+4,aim.Copy().R("Chest",1,2)));marks.Add(K(a.vfxStart+5,aim));marks.Add(K(a.vfxStart+6,aim.Copy().R("Chest",-1,-2)));marks.Add(K(a.vfxStart+7,aim));marks.Add(K(a.returnStart-.23f,prepare));
            }
            else
            {
                marks.Add(K(a.arriveTime,a.melee?windup:aim));
                foreach(var h in a.hits)
                {
                    marks.Add(K(h.time-.12f,a.melee?windup:aim));
                    marks.Add(K(h.time,a.melee?Brace(.09f).R("Spine",9).R("Chest",4,18).R("Head",-3,-10).R("UpperArm_R",-78,8,-8).R("LowerArm_R",-12):aim.Copy().R("Spine",-7).R("Chest",-5)));
                    marks.Add(K(h.time+.12f,a.melee?Brace(.07f).R("Chest",3,30).R("UpperArm_R",-68,35,-18).R("LowerArm_R",-30):aim));
                }
                marks.Add(K(a.returnStart-.22f,a.melee?windup:aim));
            }
            marks.Add(K(a.returnStart+.18f,returnPose));marks.Add(K(a.returnEnd-.22f,returnPose));marks.Add(K(a.returnEnd,prepare));
            if(a.weapon!=BattleWeaponKind.None){marks.Add(K(a.sheatheStart,draw));marks.Add(K(a.holsterTime,prepare));}
            marks.Add(K(a.duration,ready));
            return marks.OrderBy(m=>m.t).GroupBy(m=>m.t).Select(g=>g.Last()).ToArray();
        }

'''+s[end:]
f.write_text(s,encoding='utf-8')
print('Updated action definitions, actor construction, scene setup and data-timed clips.')
