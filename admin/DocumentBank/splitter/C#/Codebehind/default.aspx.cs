using System;

public class _default : System.Web.UI.Page
{
	protected System.Web.UI.WebControls.Literal treeView;
	
	void Page_Load(object sender, EventArgs e) {
		obout_ASPTreeView_2_NET.Tree oTree;
		//build TreeView
		oTree = new obout_ASPTreeView_2_NET.Tree();
			
		string Html;
		
		oTree.AddRootNode("I am Root node!", "xpPanel.gif");
		
		Html = "<span style='cursor:pointer;'>Obout Inc</span>";
		oTree.Add("root", "r1", Html, null, null, null);
		
		Html = "<span style='cursor:pointer;'>Brooklyn Bridge</span>";
		oTree.Add("root", "r2", Html, true, null, null);
		
			Html = "<span style='cursor:pointer;'>Drawing</span>";
			oTree.Add("r2", "r2_0", Html, null, null, null);
		
			Html = "<span style='cursor:pointer;'>Picture</span>";
			oTree.Add("r2", "r2_1", Html, null, null, null);
		
		Html = "<span style='cursor:pointer;'>Pictures</span>";
		oTree.Add("root", "r3", Html, true, null, null);
		
			Html = "<span style='cursor:pointer;'>Obout Inc</span>";
			oTree.Add("r3", "r3_0", Html, null, null, null);
		
			Html = "<span style='cursor:pointer;'>My Pictures</span>";
			oTree.Add("r3", "r3_1", Html, null, null, null);

		oTree.FolderIcons = "/t2/tree2/icons";
		oTree.FolderScript = "/t2/tree2/script";
		oTree.FolderStyle = "/t2/tree2/style/Classic";
		
		oTree.SelectedId = "r1";
		
		treeView.Text = oTree.HTML();
	}	
}