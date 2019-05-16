var draggedNode = "";

function ob_OnNodeDrop(src, dst, copy) {
  if (db.tree.nodeDropped) db.tree.nodeDropped(src, dst, copy);
}

function ob_OnNodeDropOutside(dst) {
  if (db.tree.nodeDroppedOutside) db.tree.nodeDroppedOutside(dst);
}


function ob_OnNodeSelect(id) {
 if (db.tree.nodeSelected) db.tree.nodeSelected(id);
}

function ob_OnNodeEdit(id, text, prevText) {
  if (db.tree.nodeRename) db.tree.nodeRename(id, text, prevText);
}

function ob_OnAddNode(parentId, childId, textOrHTML, expanded, image, subTreeURL) {
}

function ob_OnRemoveNode(id) {
}

function ob_OnNodeExpand(id, dynamic) {
  db.tree.nodeExpanded(id, dynamic);
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
  if (db.tree.checkAddNode) return db.tree.checkAddNode(parentId);
  else return true;
}

function ob_OnBeforeRemoveNode(id) {
  if (db.tree.checkRemoveNode) return db.tree.checkRemoveNode(id);
  else return true;
}

function ob_OnBeforeNodeEdit(id) {
  if (db.tree.checkNodeEdit) return db.tree.checkNodeEdit(id);
  else return true;
}

function ob_OnBeforeNodeSelect(id) {
  if (db.tree.checkNodeSelect) return db.tree.checkNodeSelect(id);
  else return true;
}

function ob_OnBeforeNodeDrop(src, dst, copy) {
  if (db.tree.checkNodeDrop) return db.tree.checkNodeDrop(src, dst, copy);
  else return true;
}

function ob_OnBeforeNodeDrag(id) {
  draggedNode = id;
  if (db.tree.checkNodeDrag) return db.tree.checkNodeDrag(id);
  else return true;
}


function ob_OnBeforeNodeDropOutside(dst) {
  return false;
}

function ob_OnBeforeNodeExpand(id, dynamic) {
  if (db.tree.checkNodeExpand) return db.tree.checkNodeExpand(id);
  else return true;
}

function ob_OnBeforeNodeCollapse(id) {
  return true;
}
