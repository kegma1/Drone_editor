using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using TMPro;
using System.Collections;

public class InspectorManager : MonoBehaviour
{
    public TimelineManager timelineManager;
    public ProjectLoader projectLoader;

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

        FlipHorizontal.isOn = data.Graphic.FlipHorizontal;
        FlipVertical.isOn = data.Graphic.FlipVertical;

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
            data.Graphic.Source = filePaths[0]; 
            currentGraphic.initSVG();
        }
	}

    public void OnChangeDroneRadius(string newValue) {
        if(projectLoader.ParsedProject != null && !isInCode) {
            projectLoader.ParsedProject.Global.DroneRadius = float.Parse(DroneRadius.text);
        }
    }

    public void OnChangePositionX(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;  
            data.Position[0] = float.Parse(PositionX.text);
        }
    }
    public void OnChangePositionY(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;  
            data.Position[1] = float.Parse(PositionY.text);
        }
    }
    public void OnChangePositionZ(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;  
            data.Position[2] = float.Parse(PositionZ.text);
        }
    }
    
    public void OnChangeRotationX(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;  
            data.Rotation[0] = float.Parse(RotationX.text);
        }
    }
    public void OnChangeRotationY(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;  
            data.Rotation[1] = float.Parse(RotationY.text);
        }
    }
    public void OnChangeRotationZ(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;  
            data.Rotation[2] = float.Parse(RotationZ.text);
        }
    }

    public void OnChangeScale(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;
            data.Graphic.Scale = float.Parse(Scale.text);
        }
    }

    public void OnChangeDuration(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;
            data.Graphic.Duration = float.Parse(Duration.text);
        }
    }

    public void OnChangeSpeed(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;
            data.Speed = float.Parse(Speed.text);
        }
    }

    public void OnChangeOutlineSpacing(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;  
            data.Graphic.OutlineSpacing = float.Parse(OutlineSpacing.text);
        }
    }
    public void OnChangeFillSpacing(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;  
            data.Graphic.FillSpacing = float.Parse(FillSpacing.text);
        }
    }
    public void OnChangeFillRotation(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;  
            data.Graphic.FillRotation = float.Parse(FillRotation.text);
        }
    }

    public void OnChangeFillOffsetX(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;  
            data.Graphic.FillOffset[0] = float.Parse(FillOffsetX.text);
        }
    }
    public void OnChangeFillOffsetY(string newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;  
            data.Graphic.FillOffset[1] = float.Parse(FillOffsetY.text);
        }
    }



    public void OnChangeFillToggle(bool newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;
            data.Graphic.Fill = FillToggle.isOn;
        }
    }
    public void OnChangeOutlineToggle(bool newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;
            data.Graphic.Outline = OutlineToggle.isOn;
        }
    }

    public void OnChangeFlipHorizontalToggle(bool newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;
            data.Graphic.FlipHorizontal = FlipHorizontal.isOn;
        }
    }

    public void OnChangeFlipVerticalToggle(bool newValue) {
        if(timelineManager.CurrentfocusedGraphic != null && !isInCode) {
            var currentGraphic = timelineManager.CurrentfocusedGraphic.GetComponent<PanelData>();
            var data = currentGraphic.animationData;
            data.Graphic.FlipVertical = FlipVertical.isOn;
        }
    }


}
