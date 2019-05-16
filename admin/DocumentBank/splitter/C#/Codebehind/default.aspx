<%@ Register Tagprefix="obspl" Namespace="OboutInc.Splitter2" Assembly="obout_Splitter2_Net" %>
<%@ Page Language="C#" Inherits="_default" Src="default.aspx.cs" %>
<html>
	<head></head>
	<body>
		<div style="width:686px;height:440px;border:1px solid #ebe9ed">
			<obspl:Splitter StyleFolder="/splitter/splitterstyles/default" id="splDV" runat="server" CookieDays="0">
				<LeftPanel WidthMin="100" WidthMax="400">
					<header height="40">
						<div style="width:100%;height:100%;background-color:#e0e6ed" class="tdText" align="center">
						<br />
						optional left header
						</div>
					</header>
					<content>
						<div style="margin:5px;"> 
							<asp:Literal id="treeView" runat="server" />
						</div>
					</content>
					<footer height="40">
						<div style="width:100%; height: 100%;background-color:#e0e6ed;" class="tdText" align="center">
						<br />
						optional left footer
						</div>
					</footer>
				</LeftPanel>
				<RightPanel>
				<header height="50">
						<div style="width:100%;height:100%;background-color:#ebe9ed" class="tdText" align="center">
						<br />
						optional right header
						</div>
					</header>
					<content>
<script>
	function AddNode()
	{
		var nodeName = document.getElementById('txtNodeName').value;
		var parentID = window.parent.tree_selected_id;
		var nodeID =  + parseInt(window.parent.ob_getChildCount(window.parent.document.getElementById(parentID)) + 1);
		ob_t2_Add(parentID, parentID + "_" + nodeID, "<span style='cursor:pointer'>" + nodeName + "</span>", null, "ball_blueS.gif", null);
	}
</script>
					<div style="font:11px verdana; color: #333333; padding-left:20px; padding-top:20px;">
						Add node to the treeview in left panel using button in right panel.
						<br />
						<br />
						Node text: <input type='text' id='txtNodeName' value='New Node'  style="font:11px verdana;" />
						&nbsp;<input type='button' value='Add' onclick='AddNode()' style="font:11px verdana;" />
						<br /><br /><br />
					</div>
					</content>
					<footer height="50">
						<div style="width:100%;height:100%;background-color:#ebe9ed" class="tdText" align="center">
						<br />
						optional right footer
						</div>
					</footer>
				</RightPanel>
			</obspl:Splitter>
		</div>
	</body>
</html>