/* $Date: 2009-04-17 23:12:31 +0200 (fr, 17 apr 2009) $    $Revision: 4820 $ */
using System;
using System.Collections;

namespace NFN {

  public interface INFNPermission {

    /// <summary></summary>
    bool GetRolePermission(String role, String action);
    /// <summary></summary>
    void SetRolePermission(String role, String action, bool permission);
    /// <summary></summary>
    ArrayList Actions { get; }
  }

}