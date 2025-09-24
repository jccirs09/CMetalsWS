import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { Input } from "./ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "./ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "./ui/tabs";
import { 
  Building, 
  MapPin, 
  Phone, 
  Mail, 
  User, 
  Package,
  TrendingUp,
  Calendar,
  Search,
  Plus,
  Edit,
  Eye,
  Clock,
  DollarSign,
  Star
} from "lucide-react";

interface Customer {
  id: string;
  name: string;
  type: "manufacturer" | "distributor" | "contractor" | "fabricator";
  status: "active" | "inactive" | "prospect";
  tier: "platinum" | "gold" | "silver" | "bronze";
  primaryContact: {
    name: string;
    title: string;
    phone: string;
    email: string;
  };
  addresses: {
    billing: {
      street: string;
      city: string;
      state: string;
      zip: string;
      country: string;
    };
    shipping: {
      street: string;
      city: string;
      state: string;
      zip: string;
      country: string;
    };
  };
  paymentTerms: string;
  creditLimit: number;
  currentBalance: number;
  totalOrders: number;
  totalRevenue: number;
  averageOrderValue: number;
  lastOrderDate: string;
  specialInstructions?: string;
  preferredDeliveryDays: string[];
  deliveryWindow: string;
  accountManager: string;
  createdDate: string;
  lastActivity: string;
}

const customerTypes = ["manufacturer", "distributor", "contractor", "fabricator"];
const customerTiers = ["platinum", "gold", "silver", "bronze"];

const sampleCustomers: Customer[] = [
  {
    id: "CUST-001",
    name: "Precision Manufacturing Inc.",
    type: "manufacturer",
    status: "active",
    tier: "platinum",
    primaryContact: {
      name: "Michael Thompson",
      title: "Purchasing Manager",
      phone: "(555) 123-4567",
      email: "m.thompson@precision-mfg.com"
    },
    addresses: {
      billing: {
        street: "123 Industrial Way",
        city: "Chicago",
        state: "IL",
        zip: "60601",
        country: "USA"
      },
      shipping: {
        street: "123 Industrial Way",
        city: "Chicago", 
        state: "IL",
        zip: "60601",
        country: "USA"
      }
    },
    paymentTerms: "Net 30",
    creditLimit: 150000,
    currentBalance: 28450,
    totalOrders: 156,
    totalRevenue: 2340000,
    averageOrderValue: 15000,
    lastOrderDate: "2024-01-15",
    specialInstructions: "Requires 24-hour advance notice for deliveries. Fragile materials need extra protection.",
    preferredDeliveryDays: ["Monday", "Tuesday", "Wednesday", "Thursday"],
    deliveryWindow: "8:00 AM - 4:00 PM",
    accountManager: "Sarah Chen",
    createdDate: "2019-03-15",
    lastActivity: "2024-01-15T10:30:00Z"
  },
  {
    id: "CUST-002", 
    name: "ABC Construction LLC",
    type: "contractor",
    status: "active",
    tier: "gold",
    primaryContact: {
      name: "Robert Johnson",
      title: "Project Manager",
      phone: "(555) 987-6543",
      email: "r.johnson@abc-construction.com"
    },
    addresses: {
      billing: {
        street: "456 Builder Blvd",
        city: "Milwaukee",
        state: "WI", 
        zip: "53201",
        country: "USA"
      },
      shipping: {
        street: "789 Job Site Ave",
        city: "Milwaukee",
        state: "WI",
        zip: "53202", 
        country: "USA"
      }
    },
    paymentTerms: "Net 45",
    creditLimit: 75000,
    currentBalance: 15680,
    totalOrders: 89,
    totalRevenue: 890000,
    averageOrderValue: 10000,
    lastOrderDate: "2024-01-12",
    preferredDeliveryDays: ["Tuesday", "Wednesday", "Thursday", "Friday"],
    deliveryWindow: "7:00 AM - 3:00 PM",
    accountManager: "John Miller",
    createdDate: "2020-07-22",
    lastActivity: "2024-01-12T14:15:00Z"
  },
  {
    id: "CUST-003",
    name: "Industrial Fabricators Corp",
    type: "fabricator", 
    status: "active",
    tier: "gold",
    primaryContact: {
      name: "Lisa Martinez",
      title: "Operations Director",
      phone: "(555) 555-0123",
      email: "l.martinez@industrial-fab.com"
    },
    addresses: {
      billing: {
        street: "789 Factory Street",
        city: "Detroit",
        state: "MI",
        zip: "48201",
        country: "USA"
      },
      shipping: {
        street: "789 Factory Street", 
        city: "Detroit",
        state: "MI",
        zip: "48201",
        country: "USA"
      }
    },
    paymentTerms: "Net 30",
    creditLimit: 100000,
    currentBalance: 42300,
    totalOrders: 134,
    totalRevenue: 1560000,
    averageOrderValue: 11640,
    lastOrderDate: "2024-01-14",
    specialInstructions: "Customer pickup only. Materials must be bundled and labeled by job number.",
    preferredDeliveryDays: ["Monday", "Wednesday", "Friday"],
    deliveryWindow: "6:00 AM - 2:00 PM",
    accountManager: "Maria Santos",
    createdDate: "2018-11-08",
    lastActivity: "2024-01-14T16:45:00Z"
  },
  {
    id: "CUST-004",
    name: "Metro Steel Distributors",
    type: "distributor",
    status: "active", 
    tier: "silver",
    primaryContact: {
      name: "David Wilson",
      title: "Inventory Manager",
      phone: "(555) 321-6789",
      email: "d.wilson@metro-steel.com"
    },
    addresses: {
      billing: {
        street: "321 Metro Avenue",
        city: "Springfield",
        state: "IL",
        zip: "62701",
        country: "USA"
      },
      shipping: {
        street: "321 Metro Avenue",
        city: "Springfield",
        state: "IL", 
        zip: "62701",
        country: "USA"
      }
    },
    paymentTerms: "Net 60",
    creditLimit: 50000,
    currentBalance: 8940,
    totalOrders: 67,
    totalRevenue: 445000,
    averageOrderValue: 6640,
    lastOrderDate: "2024-01-10",
    preferredDeliveryDays: ["Monday", "Tuesday", "Thursday", "Friday"],
    deliveryWindow: "9:00 AM - 5:00 PM",
    accountManager: "Carlos Rodriguez",
    createdDate: "2021-02-14",
    lastActivity: "2024-01-10T11:20:00Z"
  },
  {
    id: "CUST-005",
    name: "City Fabrication Works",
    type: "fabricator",
    status: "inactive",
    tier: "bronze",
    primaryContact: {
      name: "Jennifer Lee",
      title: "Purchasing Agent", 
      phone: "(555) 654-3210",
      email: "j.lee@city-fab.com"
    },
    addresses: {
      billing: {
        street: "654 City Plaza",
        city: "Rockford",
        state: "IL",
        zip: "61101",
        country: "USA"
      },
      shipping: {
        street: "654 City Plaza",
        city: "Rockford",
        state: "IL",
        zip: "61101",
        country: "USA"
      }
    },
    paymentTerms: "COD",
    creditLimit: 25000,
    currentBalance: 0,
    totalOrders: 23,
    totalRevenue: 125000,
    averageOrderValue: 5435,
    lastOrderDate: "2023-11-15",
    preferredDeliveryDays: ["Tuesday", "Thursday"],
    deliveryWindow: "10:00 AM - 3:00 PM",
    accountManager: "Jennifer Wilson",
    createdDate: "2022-08-30",
    lastActivity: "2023-11-15T13:30:00Z"
  }
];

const getTierColor = (tier: string) => {
  switch (tier) {
    case "platinum": return "bg-purple-100 text-purple-800 border-purple-200";
    case "gold": return "bg-yellow-100 text-yellow-800 border-yellow-200";
    case "silver": return "bg-gray-100 text-gray-800 border-gray-200";
    case "bronze": return "bg-orange-100 text-orange-800 border-orange-200";
    default: return "bg-gray-100 text-gray-800 border-gray-200";
  }
};

const getStatusColor = (status: string) => {
  switch (status) {
    case "active": return "bg-green-100 text-green-800 border-green-200";
    case "inactive": return "bg-red-100 text-red-800 border-red-200";
    case "prospect": return "bg-blue-100 text-blue-800 border-blue-200";
    default: return "bg-gray-100 text-gray-800 border-gray-200";
  }
};

export function Customers() {
  const [activeTab, setActiveTab] = useState("customers");
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedType, setSelectedType] = useState("all");
  const [selectedStatus, setSelectedStatus] = useState("all");
  const [selectedTier, setSelectedTier] = useState("all");

  const filteredCustomers = sampleCustomers.filter(customer => {
    const matchesSearch = customer.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         customer.id.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         customer.primaryContact.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         customer.primaryContact.email.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesType = selectedType === "all" || customer.type === selectedType;
    const matchesStatus = selectedStatus === "all" || customer.status === selectedStatus;
    const matchesTier = selectedTier === "all" || customer.tier === selectedTier;
    return matchesSearch && matchesType && matchesStatus && matchesTier;
  });

  const totalActiveRevenue = sampleCustomers
    .filter(c => c.status === 'active')
    .reduce((sum, c) => sum + c.totalRevenue, 0);

  return (
    <div className="space-y-6">
      <div>
        <h1>Customer Management</h1>
        <p className="text-muted-foreground">Manage customer information and destinations</p>
      </div>

      <Tabs value={activeTab} onValueChange={setActiveTab} className="space-y-6">
        <TabsList className="grid w-full grid-cols-3">
          <TabsTrigger value="customers" className="flex items-center gap-2">
            <Building className="h-4 w-4" />
            Customers
          </TabsTrigger>
          <TabsTrigger value="analytics" className="flex items-center gap-2">
            <TrendingUp className="h-4 w-4" />
            Analytics
          </TabsTrigger>
          <TabsTrigger value="activity" className="flex items-center gap-2">
            <Clock className="h-4 w-4" />
            Recent Activity
          </TabsTrigger>
        </TabsList>

        {/* Controls */}
        <div className="flex flex-wrap gap-4 items-center justify-between">
          <div className="flex gap-2 items-center">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search customers..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 w-64"
              />
            </div>
            <Select value={selectedType} onValueChange={setSelectedType}>
              <SelectTrigger className="w-40">
                <SelectValue placeholder="Filter by type" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Types</SelectItem>
                {customerTypes.map(type => (
                  <SelectItem key={type} value={type} className="capitalize">
                    {type}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select value={selectedStatus} onValueChange={setSelectedStatus}>
              <SelectTrigger className="w-32">
                <SelectValue placeholder="Status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="active">Active</SelectItem>
                <SelectItem value="inactive">Inactive</SelectItem>
                <SelectItem value="prospect">Prospect</SelectItem>
              </SelectContent>
            </Select>
            <Select value={selectedTier} onValueChange={setSelectedTier}>
              <SelectTrigger className="w-32">
                <SelectValue placeholder="Tier" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Tiers</SelectItem>
                {customerTiers.map(tier => (
                  <SelectItem key={tier} value={tier} className="capitalize">
                    {tier}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <Button>
            <Plus className="h-4 w-4 mr-2" />
            New Customer
          </Button>
        </div>

        <TabsContent value="customers" className="space-y-4">
          {/* Summary Cards */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-teal-100 rounded-full">
                    <Building className="h-4 w-4 text-teal-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Total Customers</p>
                    <p className="text-2xl font-medium">{filteredCustomers.length}</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-green-100 rounded-full">
                    <Star className="h-4 w-4 text-green-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Active Customers</p>
                    <p className="text-2xl font-medium">
                      {sampleCustomers.filter(c => c.status === 'active').length}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-blue-100 rounded-full">
                    <DollarSign className="h-4 w-4 text-blue-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Total Revenue</p>
                    <p className="text-2xl font-medium">${totalActiveRevenue.toLocaleString()}</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-purple-100 rounded-full">
                    <Package className="h-4 w-4 text-purple-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Total Orders</p>
                    <p className="text-2xl font-medium">
                      {sampleCustomers.reduce((sum, c) => sum + c.totalOrders, 0)}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Customers List */}
          <div className="space-y-4">
            {filteredCustomers.map(customer => (
              <Card key={customer.id} className="hover:shadow-md transition-shadow">
                <CardHeader className="pb-3">
                  <div className="flex items-start justify-between">
                    <div>
                      <CardTitle className="text-lg flex items-center gap-2">
                        <Building className="h-5 w-5" />
                        {customer.name}
                        <Badge variant="outline" className="capitalize">
                          {customer.type}
                        </Badge>
                      </CardTitle>
                      <div className="flex items-center gap-4 mt-2 text-sm text-muted-foreground">
                        <span>{customer.id}</span>
                        <span>•</span>
                        <span className="flex items-center gap-1">
                          <User className="h-3 w-3" />
                          {customer.accountManager}
                        </span>
                        <span>•</span>
                        <span className="flex items-center gap-1">
                          <Calendar className="h-3 w-3" />
                          Customer since {new Date(customer.createdDate).getFullYear()}
                        </span>
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      <Badge variant="outline" className={getTierColor(customer.tier)}>
                        {customer.tier}
                      </Badge>
                      <Badge variant="outline" className={getStatusColor(customer.status)}>
                        {customer.status}
                      </Badge>
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    {/* Contact Information */}
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div className="space-y-2">
                        <div className="flex items-center gap-2 text-sm">
                          <User className="h-4 w-4 text-muted-foreground" />
                          <div>
                            <p className="font-medium">{customer.primaryContact.name}</p>
                            <p className="text-muted-foreground">{customer.primaryContact.title}</p>
                          </div>
                        </div>
                        <div className="flex items-center gap-2 text-sm">
                          <Phone className="h-4 w-4 text-muted-foreground" />
                          <span>{customer.primaryContact.phone}</span>
                        </div>
                        <div className="flex items-center gap-2 text-sm">
                          <Mail className="h-4 w-4 text-muted-foreground" />
                          <span>{customer.primaryContact.email}</span>
                        </div>
                      </div>
                      
                      <div className="space-y-2">
                        <div className="flex items-start gap-2 text-sm">
                          <MapPin className="h-4 w-4 text-muted-foreground mt-0.5" />
                          <div>
                            <p className="font-medium">Billing Address</p>
                            <p className="text-muted-foreground">
                              {customer.addresses.billing.street}<br/>
                              {customer.addresses.billing.city}, {customer.addresses.billing.state} {customer.addresses.billing.zip}
                            </p>
                          </div>
                        </div>
                      </div>
                    </div>

                    {/* Business Stats */}
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4 p-4 bg-gray-50 rounded-lg">
                      <div className="text-center">
                        <p className="text-sm text-muted-foreground">Total Orders</p>
                        <p className="font-medium">{customer.totalOrders}</p>
                      </div>
                      <div className="text-center">
                        <p className="text-sm text-muted-foreground">Total Revenue</p>
                        <p className="font-medium">${customer.totalRevenue.toLocaleString()}</p>
                      </div>
                      <div className="text-center">
                        <p className="text-sm text-muted-foreground">Avg Order Value</p>
                        <p className="font-medium">${customer.averageOrderValue.toLocaleString()}</p>
                      </div>
                      <div className="text-center">
                        <p className="text-sm text-muted-foreground">Current Balance</p>
                        <p className={`font-medium ${customer.currentBalance > 0 ? 'text-orange-600' : 'text-green-600'}`}>
                          ${customer.currentBalance.toLocaleString()}
                        </p>
                      </div>
                    </div>

                    {/* Payment & Delivery Info */}
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                      <div className="space-y-2">
                        <div>
                          <span className="text-muted-foreground">Payment Terms:</span>
                          <p className="font-medium">{customer.paymentTerms}</p>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Credit Limit:</span>
                          <p className="font-medium">${customer.creditLimit.toLocaleString()}</p>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Last Order:</span>
                          <p className="font-medium">{new Date(customer.lastOrderDate).toLocaleDateString()}</p>
                        </div>
                      </div>
                      
                      <div className="space-y-2">
                        <div>
                          <span className="text-muted-foreground">Delivery Window:</span>
                          <p className="font-medium">{customer.deliveryWindow}</p>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Preferred Days:</span>
                          <p className="font-medium">{customer.preferredDeliveryDays.join(", ")}</p>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Last Activity:</span>
                          <p className="font-medium">{new Date(customer.lastActivity).toLocaleDateString()}</p>
                        </div>
                      </div>
                    </div>

                    {/* Special Instructions */}
                    {customer.specialInstructions && (
                      <div className="p-3 bg-blue-50 rounded-lg border border-blue-200">
                        <div className="flex items-start gap-2">
                          <Package className="h-4 w-4 text-blue-600 mt-0.5 flex-shrink-0" />
                          <div>
                            <p className="text-sm font-medium text-blue-800">Special Instructions</p>
                            <p className="text-sm text-blue-700">{customer.specialInstructions}</p>
                          </div>
                        </div>
                      </div>
                    )}

                    {/* Actions */}
                    <div className="flex justify-end gap-2 pt-3 border-t">
                      <Button variant="outline" size="sm">
                        <Eye className="h-4 w-4 mr-2" />
                        View Details
                      </Button>
                      <Button variant="outline" size="sm">
                        <Edit className="h-4 w-4 mr-2" />
                        Edit
                      </Button>
                      <Button size="sm">
                        <Plus className="h-4 w-4 mr-2" />
                        New Order
                      </Button>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </TabsContent>

        <TabsContent value="analytics" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Revenue by Customer Tier</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-3">
                  {customerTiers.map(tier => {
                    const tierCustomers = sampleCustomers.filter(c => c.tier === tier);
                    const tierRevenue = tierCustomers.reduce((sum, c) => sum + c.totalRevenue, 0);
                    const percentage = totalActiveRevenue > 0 ? (tierRevenue / totalActiveRevenue) * 100 : 0;
                    
                    return (
                      <div key={tier} className="space-y-2">
                        <div className="flex items-center justify-between">
                          <span className="capitalize flex items-center gap-2">
                            <div className={`w-3 h-3 rounded-full ${
                              tier === 'platinum' ? 'bg-purple-500' :
                              tier === 'gold' ? 'bg-yellow-500' :
                              tier === 'silver' ? 'bg-gray-400' : 'bg-orange-500'
                            }`} />
                            {tier}
                          </span>
                          <span className="font-medium">
                            ${tierRevenue.toLocaleString()} ({percentage.toFixed(1)}%)
                          </span>
                        </div>
                        <div className="w-full bg-gray-200 rounded-full h-2">
                          <div 
                            className="bg-teal-600 h-2 rounded-full" 
                            style={{ width: `${percentage}%` }}
                          />
                        </div>
                      </div>
                    );
                  })}
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Customer Distribution</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-3">
                  {customerTypes.map(type => {
                    const count = sampleCustomers.filter(c => c.type === type).length;
                    const percentage = (count / sampleCustomers.length) * 100;
                    
                    return (
                      <div key={type} className="space-y-2">
                        <div className="flex items-center justify-between">
                          <span className="capitalize">{type}</span>
                          <span className="font-medium">{count} ({percentage.toFixed(1)}%)</span>
                        </div>
                        <div className="w-full bg-gray-200 rounded-full h-2">
                          <div 
                            className="bg-teal-600 h-2 rounded-full" 
                            style={{ width: `${percentage}%` }}
                          />
                        </div>
                      </div>
                    );
                  })}
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="activity" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Recent Customer Activity</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {sampleCustomers
                  .sort((a, b) => new Date(b.lastActivity).getTime() - new Date(a.lastActivity).getTime())
                  .slice(0, 10)
                  .map(customer => (
                    <div key={customer.id} className="flex items-center justify-between p-3 border rounded-lg">
                      <div className="flex items-center gap-3">
                        <div className={`p-2 rounded-full ${
                          customer.status === 'active' ? 'bg-green-100' :
                          customer.status === 'inactive' ? 'bg-red-100' : 'bg-blue-100'
                        }`}>
                          <Building className={`h-4 w-4 ${
                            customer.status === 'active' ? 'text-green-600' :
                            customer.status === 'inactive' ? 'text-red-600' : 'text-blue-600'
                          }`} />
                        </div>
                        <div>
                          <p className="font-medium">{customer.name}</p>
                          <p className="text-sm text-muted-foreground">
                            Last order: {new Date(customer.lastOrderDate).toLocaleDateString()}
                          </p>
                        </div>
                      </div>
                      <div className="text-right">
                        <Badge variant="outline" className={getTierColor(customer.tier)}>
                          {customer.tier}
                        </Badge>
                        <p className="text-sm text-muted-foreground mt-1">
                          {new Date(customer.lastActivity).toLocaleDateString()}
                        </p>
                      </div>
                    </div>
                  ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}