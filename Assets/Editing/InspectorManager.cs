using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using TMPro;
using System.Collections;
using System.IO;

public class InspectorManager : MonoBehaviour
{
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

    private bool isInCode = false;

    public void setState(AnimationData data) {
        isInCode = true;
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
        editorGraphic.MaxDones = projectLoader.ParsedProject.Global.MaxDrones;
        

        editorGraphic.graphic = data.Graphic;
        editorGraphic.SetPos(data.Position[0], data.Position[1], data.Position[2]);
        editorGraphic.SetRot(data.Rotation[0], data.Rotation[1], data.Rotation[2]);

        isInCode = false;
    }

    public void OnClickFilePicker() {
        StartCoroutine(PickFile());
    }

    public IEnumerator PickFile() {
        FileBrowser.SetFilters( true, new FileBrowser.Filter( "Images", ".svg"));

		FileBrowser.SetDefaultFilter( ".svg" );

		FileBrowser.AddQuickLink( "Users", "C:\\Users", null );

        yield return FileBrowser.WaitForLoadDialog( FileBrowser.PickMode.Files, true, null, null, "Select Files", "Load" );
        Debug.Log( FileBrowser.Success );

        if( FileBrowser.Success )
			OnFilesSelected( FileBrowser.Result ); 
    }

    private void OnFilesSelected( string[] filePaths ) {
		Debug.Log(filePaths[0]);
        var currentGraphic = timelineManager.CurrentfocusedGraphic?.GetComponent<PanelData>();
        if (currentGraphic) {
            var data = currentGraphic.animationData;

            data.Graphic.Source = File.ReadAllText(filePaths[0]); 
            editorGraphic.graphic = data.Graphic;
            currentGraphic.initSVG();
        }
	}

    public void OnChangeDroneRadius(string newValue) {
        if(projectLoader.ParsedProject != null && !isInCode) {
            projectLoader.ParsedProject.Global.DroneRadius = float.Parse(DroneRadius.text);
            editorGraphic.pointRadius = float.Parse(DroneRadius.text);
        }
    }

    public void OnChangeMaxDrones(string newValue) {
        if(projectLoader.ParsedProject != null && !isInCode) {
            projectLoader.ParsedProject.Global.MaxDrones = int.Parse(MaxDones.text);
            editorGraphic.MaxDones = projectLoader.ParsedProject.Global.MaxDrones;
        }
    }

    public void OnChangePositionX(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;  
            data.Position[0] = float.Parse(PositionX.text);
            editorGraphic.SetPos(float.Parse(PositionX.text), null, null);
        }
    }
    public void OnChangePositionY(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;  
            data.Position[1] = float.Parse(PositionY.text);
            editorGraphic.SetPos(null, float.Parse(PositionY.text), null);
        }
    }
    public void OnChangePositionZ(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;  
            data.Position[2] = float.Parse(PositionZ.text);
            editorGraphic.SetPos(null, null, float.Parse(PositionZ.text));
        }
    }
    
    public void OnChangeRotationX(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;  
            data.Rotation[0] = float.Parse(RotationX.text);
            editorGraphic.SetRot(float.Parse(RotationX.text), null, null);
        }
    }
    public void OnChangeRotationY(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;  
            data.Rotation[1] = float.Parse(RotationY.text);
            editorGraphic.SetRot(null, float.Parse(RotationY.text), null);
        }
    }
    public void OnChangeRotationZ(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;  
            data.Rotation[2] = float.Parse(RotationZ.text);
            editorGraphic.SetRot(null, null, float.Parse(RotationZ.text));
        }
    }

    public void OnChangeScale(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;
            data.Graphic.Scale = float.Parse(Scale.text);
            editorGraphic.graphic = data.Graphic;
        }
    }

    public void OnChangeDuration(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;
            data.Graphic.Duration = float.Parse(Duration.text);
            editorGraphic.graphic = data.Graphic;
        }
    }

    public void OnChangeSpeed(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;
            data.Speed = float.Parse(Speed.text);
            editorGraphic.graphic = data.Graphic;
        }
    }

    public void OnChangeOutlineSpacing(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;  
            data.Graphic.OutlineSpacing = float.Parse(OutlineSpacing.text);
            editorGraphic.graphic = data.Graphic;
        }
    }
    public void OnChangeFillSpacing(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;  
            data.Graphic.FillSpacing = float.Parse(FillSpacing.text);
            editorGraphic.graphic = data.Graphic;
        }
    }
    public void OnChangeFillRotation(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;  
            data.Graphic.FillRotation = float.Parse(FillRotation.text);
            editorGraphic.graphic = data.Graphic;
        }
    }

    public void OnChangeFillOffsetX(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;  
            data.Graphic.FillOffset[0] = float.Parse(FillOffsetX.text);
            editorGraphic.graphic = data.Graphic;
        }
    }
    public void OnChangeFillOffsetY(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;  
            data.Graphic.FillOffset[1] = float.Parse(FillOffsetY.text);
            editorGraphic.graphic = data.Graphic;
        }
    }



    public void OnChangeFillToggle(bool newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;
            data.Graphic.Fill = FillToggle.isOn;
            editorGraphic.graphic = data.Graphic;
        }
    }
    public void OnChangeOutlineToggle(bool newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;
            data.Graphic.Outline = OutlineToggle.isOn;
            editorGraphic.graphic = data.Graphic;
        }
    }

    public void OnChangeFlipHorizontalToggle(bool newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;
            data.Graphic.FlipHorizontal = FlipHorizontal.isOn;
            editorGraphic.graphic = data.Graphic;
        }
    }

    public void OnChangeFlipVerticalToggle(bool newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;
            data.Graphic.FlipVertical = FlipVertical.isOn;
            editorGraphic.graphic = data.Graphic;
        }
    }


}
