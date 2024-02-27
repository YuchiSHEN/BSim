using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;


using System.Linq;
using BSim;

using System.Diagnostics;
using System.Windows.Forms;

/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance
{
#region Utility functions
  /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
  /// <param name="text">String to print.</param>
  private void Print(string text) { __out.Add(text); }
  /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
  /// <param name="format">String format.</param>
  /// <param name="args">Formatting parameters.</param>
  private void Print(string format, params object[] args) { __out.Add(string.Format(format, args)); }
  /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj)); }
  /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj, string method_name) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj, method_name)); }
#endregion

#region Members
  /// <summary>Gets the current Rhino document.</summary>
  private RhinoDoc RhinoDocument;
  /// <summary>Gets the Grasshopper document that owns this script.</summary>
  private GH_Document GrasshopperDocument;
  /// <summary>Gets the Grasshopper script component that owns this script.</summary>
  private IGH_Component Component; 
  /// <summary>
  /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
  /// Any subsequent call within the same solution will increment the Iteration count.
  /// </summary>
  private int Iteration;
#endregion

  /// <summary>
  /// This procedure contains the user code. Input parameters are provided as regular arguments, 
  /// Output parameters as ref arguments. You don't have to assign output parameters, 
  /// they will have a default value.
  /// </summary>
  private void RunScript(object M, ref object Views, ref object Detect, ref object Trajs, ref object TraPts)
  {
    
    List<Line> PointTo = new List<Line>();
    List<Point3d> TPts = new List<Point3d>();
    List<Line> PlCv = new List<Line>();
    List<Line> ViewLn = new List<Line>();
    DataTree<Point3d> Paths = new DataTree<Point3d>();

    ABM ABM = M as ABM;
    int k = 0;
    foreach(Person doc in ABM.PPs)
    {
      //PointTo.Add(new Line(doc.Locate, TargetPt(doc)));
      TPts.Add(TargetPt(doc));

      PlCv.AddRange(WayPath(doc.Path));

      Paths.AddRange(doc.Path, new GH_Path(k));

      ViewLn.Add(Viewline(doc));

      Point3d? CPt = ABM.Space.Obs_ClosetPt(doc.Locate, doc.SizeR * 1.005);
      if(CPt != null){PointTo.Add(new Line(doc.Locate, (Point3d) CPt));}

      k++;
    }

    Print(ABM.debug);
    Views = ViewLn;
    Detect = PointTo;
    Trajs = PlCv;
    TraPts = Paths;
  }

  // <Custom additional code> 
  
  public Line Viewline(Person P)
  {
    Line ln = new Line(P.Locate, P.Locate);
    if(P.Path.Count > 0)
    {
      ln = new Line(P.Locate, P.Path[P.step]);
    }

    return ln;
  }

  public List<Line> WayPath(List<Point3d>P)
  {
    List<Line> ln = new List<Line>();
    if(P.Count > 1)
    {
      for(int i = 0;i < P.Count - 1;i++)
      {ln.Add(new Line(P[i], P[i + 1]));}
    }

    return ln;
  }

  public Point3d TargetPt(Person P)
  {
    if(P.Path.Count > 0)
    {
      return P.Path[P.Path.Count - 1];
    }
    else
    {
      return P.Locate;
    }
  }
  // </Custom additional code> 

  private List<string> __err = new List<string>(); //Do not modify this list directly.
  private List<string> __out = new List<string>(); //Do not modify this list directly.
  private RhinoDoc doc = RhinoDoc.ActiveDoc;       //Legacy field.
  private IGH_ActiveObject owner;                  //Legacy field.
  private int runCount;                            //Legacy field.
  
  public override void InvokeRunScript(IGH_Component owner, object rhinoDocument, int iteration, List<object> inputs, IGH_DataAccess DA)
  {
    //Prepare for a new run...
    //1. Reset lists
    this.__out.Clear();
    this.__err.Clear();

    this.Component = owner;
    this.Iteration = iteration;
    this.GrasshopperDocument = owner.OnPingDocument();
    this.RhinoDocument = rhinoDocument as Rhino.RhinoDoc;

    this.owner = this.Component;
    this.runCount = this.Iteration;
    this. doc = this.RhinoDocument;

    //2. Assign input parameters
        object M = default(object);
    if (inputs[0] != null)
    {
      M = (object)(inputs[0]);
    }



    //3. Declare output parameters
      object Views = null;
  object Detect = null;
  object Trajs = null;
  object TraPts = null;


    //4. Invoke RunScript
    RunScript(M, ref Views, ref Detect, ref Trajs, ref TraPts);
      
    try
    {
      //5. Assign output parameters to component...
            if (Views != null)
      {
        if (GH_Format.TreatAsCollection(Views))
        {
          IEnumerable __enum_Views = (IEnumerable)(Views);
          DA.SetDataList(1, __enum_Views);
        }
        else
        {
          if (Views is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(1, (Grasshopper.Kernel.Data.IGH_DataTree)(Views));
          }
          else
          {
            //assign direct
            DA.SetData(1, Views);
          }
        }
      }
      else
      {
        DA.SetData(1, null);
      }
      if (Detect != null)
      {
        if (GH_Format.TreatAsCollection(Detect))
        {
          IEnumerable __enum_Detect = (IEnumerable)(Detect);
          DA.SetDataList(2, __enum_Detect);
        }
        else
        {
          if (Detect is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(2, (Grasshopper.Kernel.Data.IGH_DataTree)(Detect));
          }
          else
          {
            //assign direct
            DA.SetData(2, Detect);
          }
        }
      }
      else
      {
        DA.SetData(2, null);
      }
      if (Trajs != null)
      {
        if (GH_Format.TreatAsCollection(Trajs))
        {
          IEnumerable __enum_Trajs = (IEnumerable)(Trajs);
          DA.SetDataList(3, __enum_Trajs);
        }
        else
        {
          if (Trajs is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(3, (Grasshopper.Kernel.Data.IGH_DataTree)(Trajs));
          }
          else
          {
            //assign direct
            DA.SetData(3, Trajs);
          }
        }
      }
      else
      {
        DA.SetData(3, null);
      }
      if (TraPts != null)
      {
        if (GH_Format.TreatAsCollection(TraPts))
        {
          IEnumerable __enum_TraPts = (IEnumerable)(TraPts);
          DA.SetDataList(4, __enum_TraPts);
        }
        else
        {
          if (TraPts is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(4, (Grasshopper.Kernel.Data.IGH_DataTree)(TraPts));
          }
          else
          {
            //assign direct
            DA.SetData(4, TraPts);
          }
        }
      }
      else
      {
        DA.SetData(4, null);
      }

    }
    catch (Exception ex)
    {
      this.__err.Add(string.Format("Script exception: {0}", ex.Message));
    }
    finally
    {
      //Add errors and messages... 
      if (owner.Params.Output.Count > 0)
      {
        if (owner.Params.Output[0] is Grasshopper.Kernel.Parameters.Param_String)
        {
          List<string> __errors_plus_messages = new List<string>();
          if (this.__err != null) { __errors_plus_messages.AddRange(this.__err); }
          if (this.__out != null) { __errors_plus_messages.AddRange(this.__out); }
          if (__errors_plus_messages.Count > 0) 
            DA.SetDataList(0, __errors_plus_messages);
        }
      }
    }
  }
}