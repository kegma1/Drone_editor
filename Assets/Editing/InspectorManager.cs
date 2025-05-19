using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using TMPro;
using System.Collections;
using System.IO;
using System;

public class InspectorManager : MonoBehaviour
{
    // håndterer all ui-en som er i inspector panelet
    public TimelineManager timelineManager;
    public ProjectLoader projectLoader;
    public EditorGraphic editorGraphic;

    public TMP_InputField PositionX;
    public TMP_InputField PositionY;
    public TMP_InputField PositionZ;
    
    public TMP_InputField RotationX;
    public TMP_InputField RotationY;
    public TMP_InputField RotationZ;

    public TMP_InputField Duration;
    public TMP_InputField Speed;
    public TMP_InputField Scale;

    public Toggle OutlineToggle;
    public TMP_InputField OutlineSpacing;

    public Toggle FillToggle;
    public TMP_InputField FillSpacing;
    public TMP_InputField FillRotation;

    public TMP_InputField FillOffsetX;
    public TMP_InputField FillOffsetY;
    public TMP_InputField DroneRadius;
    public TMP_InputField MaxDones;

    public Toggle FlipHorizontal;
    public Toggle FlipVertical;
    public Toggle IsLooping;

    private bool isInCode = false;

    public ErrorManager errorManager; // Referanse til objekt brukt for å vise feilmeldinger til brukeren

    // oppdaterer alle tekstboksene med verdiene fra det valgte bildet.
    public void setState(AnimationData data)
    {
        isInCode = true; // setter denne sann for å hindre en loop med hendelseslytter
        PositionX.text = data.Position[0].ToString();
        PositionY.text = data.Position[1].ToString();
        PositionZ.text = data.Position[2].ToString();

        RotationX.text = data.Rotation[0].ToString();
        RotationY.text = data.Rotation[1].ToString();
        RotationZ.text = data.Rotation[2].ToString();

        Duration.text = data.Graphic.Duration.ToString();
        Speed.text = data.Speed.ToString();
        Scale.text = data.Graphic.Scale.ToString();

        OutlineToggle.isOn = data.Graphic.Outline;
        OutlineSpacing.text = data.Graphic.OutlineSpacing.ToString();

        FillToggle.isOn = data.Graphic.Fill;
        FillSpacing.text = data.Graphic.FillSpacing.ToString();
        FillRotation.text = data.Graphic.FillRotation.ToString();

        FillOffsetX.text = data.Graphic.FillOffset[0].ToString();
        FillOffsetY.text = data.Graphic.FillOffset[1].ToString();

        DroneRadius.text = projectLoader.ParsedProject.Global.DroneRadius.ToString();
        MaxDones.text = projectLoader.ParsedProject.Global.MaxDrones.ToString();


        FlipHorizontal.isOn = data.Graphic.FlipHorizontal;
        FlipVertical.isOn = data.Graphic.FlipVertical;

        editorGraphic.pointRadius = projectLoader.ParsedProject.Global.DroneRadius;
        editorGraphic.MaxDrones = projectLoader.ParsedProject.Global.MaxDrones;

        IsLooping.isOn = projectLoader.ParsedProject.Global.IsLooping;

        editorGraphic.graphic = data.Graphic;
        editorGraphic.SetPos(data.Position[0], data.Position[1], data.Position[2]);
        editorGraphic.SetRot(data.Rotation[0], data.Rotation[1], data.Rotation[2]);

        isInCode = false;
    }

    public void OnClickFilePicker() {
        StartCoroutine(PickFile());
    }

    // lar brukeren plukke ut en svg fil
    public IEnumerator PickFile()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".svg"));

        FileBrowser.SetDefaultFilter(".svg");

        FileBrowser.AddQuickLink("Users", "C:\\Users", null);

        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, true, null, null, "Select Files", "Load");
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
            OnFilesSelected(FileBrowser.Result);
        else
            errorManager.DisplayError("No file selected", 2);
    }

    private void OnFilesSelected( string[] filePaths ) {
		Debug.Log(filePaths[0]);
        var currentGraphic = timelineManager.CurrentfocusedGraphic?.GetComponent<PanelData>();
        if (currentGraphic) {
            var data = currentGraphic.animationData;

            try {
                data.Graphic.Source = File.ReadAllText(filePaths[0]); 
            } catch(Exception) {
                errorManager.DisplayError("ERROR: Unable to read file", 5);
                return;
            }
            editorGraphic.graphic = data.Graphic;
            currentGraphic.initSVG();
        }
	}

    // nesten alle tekstboksene og knappene må gjøre dette.
    //                              V- dette er hva som skal skje når en gitt tekst boks er endret
    private void ModifyAnimationData(Action<AnimationData> modification)
    {
        if (timelineManager.CurrentfocusedGraphic != null && !isInCode)
        {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;
            modification(data);
        }
    }

    private void ModifyGlobalData(Action modification)
    {
        if(projectLoader.ParsedProject != null && !isInCode) {
            modification();
        }
    }

    // dette stemmer for alle men newValue er egentlig ikke new value, men en statisk verdi du kan sette i unity.
    // derfor henter alle den nye verdien direkte fra ui-elementet sitt.
    // På mange kjøres koden "editorGraphic.graphic = data.Graphic;". 
    // dette oppdaterer grafikken slik at den matcher de nye verdien.
    public void OnChangeDroneRadius(string newValue)
    {
        ModifyGlobalData(() =>
        {
            projectLoader.ParsedProject.Global.DroneRadius = float.Parse(DroneRadius.text);
            editorGraphic.pointRadius = float.Parse(DroneRadius.text);
            editorGraphic.GeneratePointsFromPath();
        });
    }

    public void OnChangeMaxDrones(string newValue) {
        ModifyGlobalData(() => {
            projectLoader.ParsedProject.Global.MaxDrones = int.Parse(MaxDones.text);
            editorGraphic.MaxDrones = projectLoader.ParsedProject.Global.MaxDrones;
            editorGraphic.GeneratePointsFromPath(); 
            editorGraphic.CleanDrones();
        });
    }
    
    public void OnChangeIsLoopingToggle(bool newValue) {
        ModifyGlobalData(() => {
            projectLoader.ParsedProject.Global.IsLooping = IsLooping.isOn;
        });
    }

    public void OnChangePositionX(string newValue)
    {
        ModifyAnimationData(data =>
        {
            data.Position[0] = float.Parse(PositionX.text);
            editorGraphic.SetPos(float.Parse(PositionX.text), null, null);
        });
    }
    public void OnChangePositionY(string newValue)
    {
        ModifyAnimationData(data => {
            data.Position[1] = float.Parse(PositionY.text);
            editorGraphic.SetPos(null, float.Parse(PositionY.text), null);
        });
    }
    public void OnChangePositionZ(string newValue) {
        ModifyAnimationData(data => {  
            data.Position[2] = float.Parse(PositionZ.text);
            editorGraphic.SetPos(null, null, float.Parse(PositionZ.text));
        });
    }
    
    public void OnChangeRotationX(string newValue) {
        ModifyAnimationData(data => {  
            data.Rotation[0] = float.Parse(RotationX.text);
            editorGraphic.SetRot(float.Parse(RotationX.text), null, null);
        });
    }
    public void OnChangeRotationY(string newValue) {
        ModifyAnimationData(data => {  
            data.Rotation[1] = float.Parse(RotationY.text);
            editorGraphic.SetRot(null, float.Parse(RotationY.text), null);
        });
    }
    public void OnChangeRotationZ(string newValue) {
        ModifyAnimationData(data => {  
            data.Rotation[2] = float.Parse(RotationZ.text);
            editorGraphic.SetRot(null, null, float.Parse(RotationZ.text));
        });
    }

    public void OnChangeScale(string newValue) {
        ModifyAnimationData(data => {
            data.Graphic.Scale = float.Parse(Scale.text);
            editorGraphic.graphic = data.Graphic;
        });
    }

    public void OnChangeDuration(string newValue) {
        ModifyAnimationData(data => {
            data.Graphic.Duration = float.Parse(Duration.text);
            editorGraphic.graphic = data.Graphic;
        });
    }

    public void OnChangeSpeed(string newValue) {
        ModifyAnimationData(data => {
            data.Speed = float.Parse(Speed.text);
            editorGraphic.graphic = data.Graphic;
        });
    }

    public void OnChangeOutlineSpacing(string newValue) {
        ModifyAnimationData(data => {  
            data.Graphic.OutlineSpacing = float.Parse(OutlineSpacing.text);
            editorGraphic.graphic = data.Graphic;
        });
    }
    public void OnChangeFillSpacing(string newValue) {
        ModifyAnimationData(data => {  
            data.Graphic.FillSpacing = float.Parse(FillSpacing.text);
            editorGraphic.graphic = data.Graphic;
        });
    }
    public void OnChangeFillRotation(string newValue) {
        ModifyAnimationData(data => {  
            data.Graphic.FillRotation = float.Parse(FillRotation.text);
            editorGraphic.graphic = data.Graphic;
        });
    }

    public void OnChangeFillOffsetX(string newValue) {
        ModifyAnimationData(data => {  
            data.Graphic.FillOffset[0] = float.Parse(FillOffsetX.text);
            editorGraphic.graphic = data.Graphic;
        });
    }
    public void OnChangeFillOffsetY(string newValue) {
        ModifyAnimationData(data => {  
            data.Graphic.FillOffset[1] = float.Parse(FillOffsetY.text);
            editorGraphic.graphic = data.Graphic;
        });
    }


    public void OnChangeFillToggle(bool newValue) {
        ModifyAnimationData(data => {
            data.Graphic.Fill = FillToggle.isOn;
            editorGraphic.graphic = data.Graphic;
        });
    }
    public void OnChangeOutlineToggle(bool newValue) {
        ModifyAnimationData(data => {
            data.Graphic.Outline = OutlineToggle.isOn;
            editorGraphic.graphic = data.Graphic;
        });
    }

    public void OnChangeFlipHorizontalToggle(bool newValue) {
        ModifyAnimationData(data => {
            data.Graphic.FlipHorizontal = FlipHorizontal.isOn;
            editorGraphic.graphic = data.Graphic;
        });
    }

    public void OnChangeFlipVerticalToggle(bool newValue) {
        ModifyAnimationData(data => {
            data.Graphic.FlipVertical = FlipVertical.isOn;
            editorGraphic.graphic = data.Graphic;
        });
    }

}
