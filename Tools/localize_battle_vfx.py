from pathlib import Path
p=Path('Assets/TurnBasedBattle/Scripts/Runtime/EarthPrisonController.cs');s=p.read_text(encoding='utf-8')
s=s.replace('line.positionCount=line==groundRing?97:33;', 'line.useWorldSpace=false;line.positionCount=line==groundRing?97:33;')
s=s.replace('foreach(var ps in particles) ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);','foreach(var ps in particles){ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=ps.main;main.simulationSpace=ParticleSystemSimulationSpace.Custom;main.customSimulationSpace=transform;main.scalingMode=ParticleSystemScalingMode.Hierarchy;}')
s=s.replace('Vector3 origin=source!=null?source.position:area.position+Vector3.left*10+Vector3.up*1.8f;', 'Vector3 origin=source!=null?transform.InverseTransformPoint(source.position):area.localPosition+Vector3.left*10+Vector3.up*1.8f;')
s=s.replace('shape.position=position;', 'shape.position=transform.TransformPoint(position);')
s=s.replace('area.position+','area.localPosition+')
s=s.replace('chargeSparks.transform.position=origin','chargeSparks.transform.localPosition=origin')
s=s.replace('transferSparks.transform.position=Vector3.Lerp(origin,end,.55f)','transferSparks.transform.localPosition=Vector3.Lerp(origin,end,.55f)')
for name in ['impactSparks','impactDust','residualDust']:
    s=s.replace(name+'.transform.position=',name+'.transform.localPosition=')
s=s.replace('tips[i].position','transform.InverseTransformPoint(tips[i].position)').replace('tips[j].position','transform.InverseTransformPoint(tips[j].position)').replace('tips[i+3].position','transform.InverseTransformPoint(tips[i+3].position)')
s=s.replace('pillars[i].position+Vector3.up*.15f','transform.InverseTransformPoint(pillars[i].position)+Vector3.up*.15f')
s=s.replace('tipSparks[i].transform.position=', 'tipSparks[i].transform.localPosition=')
s=s.replace('        void OnDisable()', '''        void LateUpdate()
        {
            if(!Active || viewCamera==null)return;
            orb.rotation=orbGlow.rotation=impactGlow.rotation=viewCamera.transform.rotation;
            foreach(var glow in tipGlows)glow.rotation=viewCamera.transform.rotation;
        }
        void OnDisable()''')
p.write_text(s,encoding='utf-8')
p=Path('Assets/TurnBasedBattle/Scripts/Runtime/BattleEffects.cs');s=p.read_text(encoding='utf-8')
s=s.replace('        public void BeginAction(','''        void Awake()
        {
            foreach(var p in impactPool){var main=p.main;main.simulationSpace=ParticleSystemSimulationSpace.Custom;main.customSimulationSpace=transform;main.scalingMode=ParticleSystemScalingMode.Hierarchy;}
            foreach(var line in projectiles)line.useWorldSpace=false;
        }
        void SetLine(LineRenderer line)
        {
            for(int i=0;i<linePoints.Length;i++)linePoints[i]=transform.InverseTransformPoint(linePoints[i]);
            line.SetPositions(linePoints);
        }
        public void BeginAction(''')
s=s.replace('line.SetPositions(linePoints);','SetLine(line);')
# Restore actual setter inside helper.
s=s.replace('linePoints[i]=transform.InverseTransformPoint(linePoints[i]);\n            SetLine(line);','linePoints[i]=transform.InverseTransformPoint(linePoints[i]);\n            line.SetPositions(linePoints);')
s=s.replace('Vector3.up*.8f+Vector3.back*.25f','target.transform.TransformVector(new Vector3(0,.8f,-.25f))')
s=s.replace('+Vector3.up*Mathf.Sin(u*Mathf.PI)*Mathf.Sin(time*50+i)*.03f','+source.transform.up*Mathf.Sin(u*Mathf.PI)*Mathf.Sin(time*50+i)*.03f*Mathf.Abs(source.transform.lossyScale.y)')
p.write_text(s,encoding='utf-8')
print('VFX coordinate system follows board transforms and scale.')
