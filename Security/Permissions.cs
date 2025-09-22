using System.Collections.Generic;

namespace CMetalsWS.Security
{
    public static class Permissions
    {
        public const string ClaimType = "permission";

        public static class Users
        {
            public const string View = "Users.View";
            public const string Add = "Users.Add";
            public const string Edit = "Users.Edit";
            public const string Delete = "Users.Delete";
        }

        public static class Roles
        {
            public const string View = "Roles.View";
            public const string Add = "Roles.Add";
            public const string Edit = "Roles.Edit";
            public const string Delete = "Roles.Delete";
        }

        public static class Branches
        {
            public const string View = "Branches.View";
            public const string Add = "Branches.Add";
            public const string Edit = "Branches.Edit";
            public const string Delete = "Branches.Delete";
        }

        public static class Machines
        {
            public const string View = "Machines.View";
            public const string Add = "Machines.Add";
            public const string Edit = "Machines.Edit";
            public const string Delete = "Machines.Delete";
        }

        public static class Trucks
        {
            public const string View = "Trucks.View";
            public const string Add = "Trucks.Add";
            public const string Edit = "Trucks.Edit";
            public const string Delete = "Trucks.Delete";
        }

        public static class WorkOrders
        {
            public const string View = "WorkOrders.View";
            public const string Add = "WorkOrders.Add";
            public const string Edit = "WorkOrders.Edit";
            public const string Delete = "WorkOrders.Delete";
            public const string Schedule = "WorkOrders.Schedule";
            public const string Process = "WorkOrders.Process";
            public const string Approve = "WorkOrders.Approve";
        }

        public static class PickingLists
        {
            public const string View = "PickingLists.View";
            public const string Add = "PickingLists.Add";
            public const string Edit = "PickingLists.Edit";
            public const string Delete = "PickingLists.Delete";
            public const string Assign = "PickingLists.Assign";
            public const string Dispatch = "PickingLists.Dispatch";
            public const string ManageLoads = "PickingLists.ManageLoads";
        }

        public static class Dashboards
        {
            public const string View = "Dashboards.View";
        }

        public static class Reports
        {
            public const string View = "Reports.View";
            public const string Export = "Reports.Export";
        }

        public static class Customers
        {
            public const string View = "Customers.View";
            public const string Add = "Customers.Add";
            public const string Edit = "Customers.Edit";
            public const string Delete = "Customers.Delete";
            public const string Import = "Customers.Import";
            public const string Manage = "Customers.Manage";
        }

        public static class Shifts
        {
            public const string View = "Shifts.View";
            public const string Add = "Shifts.Add";
            public const string Edit = "Shifts.Edit";
            public const string Delete = "Shifts.Delete";
        }

        public static IEnumerable<string> All()
        {
            yield return Users.View; yield return Users.Add; yield return Users.Edit; yield return Users.Delete;
            yield return Roles.View; yield return Roles.Add; yield return Roles.Edit; yield return Roles.Delete;
            yield return Branches.View; yield return Branches.Add; yield return Branches.Edit; yield return Branches.Delete;
            yield return Machines.View; yield return Machines.Add; yield return Machines.Edit; yield return Machines.Delete;
            yield return Trucks.View; yield return Trucks.Add; yield return Trucks.Edit; yield return Trucks.Delete;
            yield return WorkOrders.View; yield return WorkOrders.Add; yield return WorkOrders.Edit; yield return WorkOrders.Delete;
            yield return WorkOrders.Schedule; yield return WorkOrders.Process; yield return WorkOrders.Approve;
            yield return PickingLists.View; yield return PickingLists.Add; yield return PickingLists.Edit; yield return PickingLists.Delete;
            yield return PickingLists.Assign; yield return PickingLists.Dispatch; yield return PickingLists.ManageLoads;
            yield return Dashboards.View;
            yield return Reports.View; yield return Reports.Export;
            yield return Customers.View; yield return Customers.Add; yield return Customers.Edit; yield return Customers.Delete; yield return Customers.Import; yield return Customers.Manage;
            yield return Shifts.View; yield return Shifts.Add; yield return Shifts.Edit; yield return Shifts.Delete;
        }
    }
}
