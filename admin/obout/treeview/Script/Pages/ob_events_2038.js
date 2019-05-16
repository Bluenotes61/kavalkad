var draggedNode = "";

function ob_OnNodeDrop(src, dst, copy) {
  if (typeof(NodeDropped) != "undefined")
    NodeDropped(src, dst, copy);
}

function ob_OnNodeDropOutside(dst) {
  if (typeof(NodeDroppedOutside) != "undefined")
    NodeDroppedOutside(dst);
}


function ob_OnNodeSelect(id) {
 if (typeof(NodeSelected) != "undefined")
   NodeSelected(id);
}

function ob_OnNodeEdit(id, text, prevText) {
  if (typeof(NodeRename) != "undefined")
    NodeRename(id, text, prevText);
}

function ob_OnAddNode(parentId, childId, textOrHTML, expanded, image, subTreeURL) {
}

function ob_OnRemoveNode(id) {
}

function ob_OnNodeExpand(id, dynamic) {
}

function ob_OnNodeCollapse(id) {
}

function ob_OnMoveNodeUp(node_up_id, node_down_id) {
}

function ob_OnMoveNodeDown(node_down_id, node_up_id) {
}

/*
  Pre-events.
  Use them to implement your own validation for such operations as add, remove, edit
*/

function ob_OnBeforeAddNode(parentId, childId, textOrHTML, expanded, image, subTreeURL) {
  if (typeof(CheckAddNode) != "undefined")
    return CheckAddNode(parentId);
  else
    return true;
}

function ob_OnBeforeRemoveNode(id) {
  if (typeof(CheckRemoveNode) != "undefined")
    return CheckRemoveNode(id);
  else
    return true;
}

function ob_OnBeforeNodeEdit(id) {
  if (typeof(CheckNodeEdit) != "undefined")
    return CheckNodeEdit(id);
  else
    return true;
}

function ob_OnBeforeNodeSelect(id) {
  if (typeof(CheckNodeSelect) != "undefined")
    return CheckNodeSelect(id);
  else
    return true;
}

function ob_OnBeforeNodeDrop(src, dst, copy) {
  if (typeof(CheckNodeDrop) != "undefined")
    return CheckNodeDrop(src, dst, copy);
  else
    return true;
}

function ob_OnBeforeNodeDrag(id) {
  draggedNode = id;
  if (typeof(CheckNodeDrag) != "undefined")
    return CheckNodeDrag(id);
  else
    return true;
}


function ob_OnBeforeNodeDropOutside(src) {
  if (typeof(CheckNodeDropOutside) != "undefined")
    return CheckNodeDropOutside(src);
  else
    return false;
}

function ob_OnBeforeNodeExpand(id, dynamic) {
  if (typeof(CheckNodeExpand) != "undefined")
    return CheckNodeExpand(id);
  else
    return true;
}

function ob_OnBeforeNodeCollapse(id) {
  return true;
}
