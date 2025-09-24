import { useState } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { Separator } from "./ui/separator";
import { Progress } from "./ui/progress";
import { Textarea } from "./ui/textarea";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger } from "./ui/dialog";
import {
  MapPin,
  Navigation,
  Package,
  CheckCircle,
  Clock,
  Phone,
  FileSignature,
  Camera,
  AlertCircle,
  ChevronRight,
  Truck,
  User
} from "lucide-react";
import { StatusChip } from "./StatusChip";

interface Stop {
  id: string;
  customer: string;
  address: string;
  city: string;
  state: string;
  zip: string;
  phone: string;
  contactPerson: string;
  orders: Array<{
    id: string;
    items: number;
    weight: number;
    specialInstructions?: string;
  }>;
  status: "pending" | "en-route" | "arrived" | "delivered";
  deliveryWindow: string;
  signature?: string;
  deliveryNotes?: string;
  photos?: string[];
}

interface Load {
  id: string;
  truckNumber: string;
  driverName: string;
  totalWeight: number;
  capacity: number;
  stops: Stop[];
  status: "assigned" | "in-progress" | "completed";
  departureTime?: string;
}

const mockLoad: Load = {
  id: "L-2024-001",
  truckNumber: "T-105",
  driverName: "Robert Martinez",
  totalWeight: 18500,
  capacity: 26000,
  status: "in-progress",
  departureTime: "08:30 AM",
  stops: [
    {
      id: "stop-1",
      customer: "Industrial Metals Co",
      address: "1245 Manufacturing Blvd",
      city: "Chicago",
      state: "IL",
      zip: "60608",
      phone: "(312) 555-0123",
      contactPerson: "Mike Thompson",
      orders: [
        { id: "SO-45621", items: 8, weight: 4200 },
        { id: "SO-45630", items: 12, weight: 6800 }
      ],
      status: "delivered",
      deliveryWindow: "10:00 AM - 12:00 PM",
      signature: "M. Thompson",
      deliveryNotes: "Delivered to loading dock. All items inspected and accepted."
    },
    {
      id: "stop-2", 
      customer: "Precision Parts LLC",
      address: "890 Industrial Park Dr",
      city: "Aurora",
      state: "IL", 
      zip: "60502",
      phone: "(630) 555-0198",
      contactPerson: "Sarah Kim",
      orders: [
        { id: "SO-45622", items: 6, weight: 3200 }
      ],
      status: "arrived",
      deliveryWindow: "1:00 PM - 3:00 PM"
    },
    {
      id: "stop-3",
      customer: "Metro Construction",
      address: "567 Commerce Ave",
      city: "Naperville", 
      state: "IL",
      zip: "60540",
      phone: "(630) 555-0167",
      contactPerson: "David Chen",
      orders: [
        { id: "SO-45623", items: 10, weight: 4300, specialInstructions: "Crane required for unloading" }
      ],
      status: "pending",
      deliveryWindow: "3:30 PM - 5:30 PM"
    }
  ]
};

export function MobileDriverApp() {
  const [selectedStop, setSelectedStop] = useState<Stop | null>(null);
  const [signatureMode, setSignatureMode] = useState(false);
  const [deliveryNotes, setDeliveryNotes] = useState("");

  const completedStops = mockLoad.stops.filter(s => s.status === "delivered").length;
  const totalStops = mockLoad.stops.length;
  const progressPercentage = (completedStops / totalStops) * 100;

  const updateStopStatus = (stopId: string, status: Stop["status"]) => {
    // In real app, this would update the backend
    console.log(`Updating stop ${stopId} to status: ${status}`);
  };

  const getStatusColor = (status: Stop["status"]) => {
    switch (status) {
      case "delivered": return "bg-green-100 text-green-700";
      case "arrived": return "bg-blue-100 text-blue-700";
      case "en-route": return "bg-teal-100 text-teal-700";
      default: return "bg-gray-100 text-gray-700";
    }
  };

  const getStatusIcon = (status: Stop["status"]) => {
    switch (status) {
      case "delivered": return <CheckCircle className="h-4 w-4" />;
      case "arrived": return <MapPin className="h-4 w-4" />;
      case "en-route": return <Navigation className="h-4 w-4" />;
      default: return <Clock className="h-4 w-4" />;
    }
  };

  return (
    <div className="max-w-sm mx-auto bg-white min-h-screen">
      {/* Header */}
      <div className="bg-teal-600 text-white p-4">
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-2">
            <Truck className="h-6 w-6" />
            <div>
              <h1 className="text-lg font-semibold">Load {mockLoad.id}</h1>
              <p className="text-sm opacity-90">Truck {mockLoad.truckNumber}</p>
            </div>
          </div>
          <div className="text-right">
            <p className="text-sm opacity-90">Driver</p>
            <p className="font-medium">{mockLoad.driverName}</p>
          </div>
        </div>

        {/* Progress */}
        <div className="space-y-2">
          <div className="flex justify-between text-sm">
            <span>Progress</span>
            <span>{completedStops} of {totalStops} stops</span>
          </div>
          <Progress value={progressPercentage} className="h-2 bg-teal-500" />
        </div>

        {/* Load Summary */}
        <div className="flex justify-between text-sm mt-4 pt-4 border-t border-teal-500">
          <div>
            <p className="opacity-90">Total Weight</p>
            <p className="font-medium">{mockLoad.totalWeight.toLocaleString()} lbs</p>
          </div>
          <div>
            <p className="opacity-90">Capacity</p>
            <p className="font-medium">{mockLoad.capacity.toLocaleString()} lbs</p>
          </div>
          <div>
            <p className="opacity-90">Departed</p>
            <p className="font-medium">{mockLoad.departureTime}</p>
          </div>
        </div>
      </div>

      {/* Stops List */}
      <div className="p-4 space-y-4">
        <h2 className="font-semibold text-gray-900">Delivery Stops</h2>
        
        {mockLoad.stops.map((stop, index) => (
          <Card key={stop.id} className="overflow-hidden">
            <CardContent className="p-0">
              <div className="p-4">
                <div className="flex items-start justify-between mb-3">
                  <div className="flex items-center gap-2">
                    <div className="flex h-8 w-8 items-center justify-center rounded-full bg-gray-100 text-sm font-medium">
                      {index + 1}
                    </div>
                    <div>
                      <h3 className="font-medium">{stop.customer}</h3>
                      <p className="text-sm text-gray-600">{stop.city}, {stop.state}</p>
                    </div>
                  </div>
                  <Badge className={getStatusColor(stop.status)}>
                    {getStatusIcon(stop.status)}
                    <span className="ml-1 capitalize">{stop.status}</span>
                  </Badge>
                </div>

                <div className="space-y-2 text-sm">
                  <div className="flex items-center gap-2 text-gray-600">
                    <MapPin className="h-4 w-4" />
                    <span>{stop.address}</span>
                  </div>
                  <div className="flex items-center gap-2 text-gray-600">
                    <Clock className="h-4 w-4" />
                    <span>Window: {stop.deliveryWindow}</span>
                  </div>
                  <div className="flex items-center gap-2 text-gray-600">
                    <Package className="h-4 w-4" />
                    <span>
                      {stop.orders.reduce((sum, order) => sum + order.items, 0)} items • {" "}
                      {stop.orders.reduce((sum, order) => sum + order.weight, 0).toLocaleString()} lbs
                    </span>
                  </div>
                </div>

                {/* Action Buttons */}
                <div className="flex gap-2 mt-4">
                  <Button 
                    variant="outline" 
                    size="sm" 
                    className="flex-1"
                    onClick={() => window.open(`tel:${stop.phone}`)}
                  >
                    <Phone className="h-4 w-4 mr-1" />
                    Call
                  </Button>
                  <Button 
                    variant="outline" 
                    size="sm" 
                    className="flex-1"
                    onClick={() => window.open(`https://maps.google.com/?q=${encodeURIComponent(stop.address + " " + stop.city + " " + stop.state)}`)}
                  >
                    <Navigation className="h-4 w-4 mr-1" />
                    Navigate
                  </Button>
                  <Dialog>
                    <DialogTrigger asChild>
                      <Button 
                        variant="outline" 
                        size="sm"
                        onClick={() => setSelectedStop(stop)}
                      >
                        <ChevronRight className="h-4 w-4" />
                      </Button>
                    </DialogTrigger>
                    <DialogContent className="max-w-sm mx-4">
                      <DialogHeader>
                        <DialogTitle>{stop.customer}</DialogTitle>
                        <DialogDescription>
                          Stop {index + 1} details and actions
                        </DialogDescription>
                      </DialogHeader>
                      
                      <div className="space-y-4">
                        {/* Contact Info */}
                        <div className="space-y-2">
                          <h4 className="font-medium">Contact Information</h4>
                          <div className="text-sm space-y-1">
                            <div className="flex items-center gap-2">
                              <User className="h-4 w-4 text-gray-400" />
                              <span>{stop.contactPerson}</span>
                            </div>
                            <div className="flex items-center gap-2">
                              <Phone className="h-4 w-4 text-gray-400" />
                              <span>{stop.phone}</span>
                            </div>
                            <div className="flex items-start gap-2">
                              <MapPin className="h-4 w-4 text-gray-400 mt-0.5" />
                              <div>
                                <p>{stop.address}</p>
                                <p>{stop.city}, {stop.state} {stop.zip}</p>
                              </div>
                            </div>
                          </div>
                        </div>

                        <Separator />

                        {/* Orders */}
                        <div className="space-y-2">
                          <h4 className="font-medium">Orders</h4>
                          {stop.orders.map(order => (
                            <div key={order.id} className="p-2 bg-gray-50 rounded text-sm">
                              <div className="flex justify-between mb-1">
                                <span className="font-medium">{order.id}</span>
                                <span>{order.items} items</span>
                              </div>
                              <div className="flex justify-between text-gray-600">
                                <span>Weight: {order.weight.toLocaleString()} lbs</span>
                              </div>
                              {order.specialInstructions && (
                                <div className="mt-2 flex items-start gap-2 text-orange-700">
                                  <AlertCircle className="h-4 w-4 mt-0.5 flex-shrink-0" />
                                  <span>{order.specialInstructions}</span>
                                </div>
                              )}
                            </div>
                          ))}
                        </div>

                        <Separator />

                        {/* Status Actions */}
                        <div className="space-y-3">
                          <h4 className="font-medium">Update Status</h4>
                          
                          {stop.status === "pending" && (
                            <Button 
                              className="w-full bg-teal-600 hover:bg-teal-700"
                              onClick={() => updateStopStatus(stop.id, "en-route")}
                            >
                              <Navigation className="h-4 w-4 mr-2" />
                              Mark En Route
                            </Button>
                          )}
                          
                          {stop.status === "en-route" && (
                            <Button 
                              className="w-full bg-blue-600 hover:bg-blue-700"
                              onClick={() => updateStopStatus(stop.id, "arrived")}
                            >
                              <MapPin className="h-4 w-4 mr-2" />
                              Mark Arrived
                            </Button>
                          )}
                          
                          {stop.status === "arrived" && (
                            <div className="space-y-3">
                              <Textarea
                                placeholder="Delivery notes (optional)"
                                value={deliveryNotes}
                                onChange={(e) => setDeliveryNotes(e.target.value)}
                                className="resize-none"
                                rows={3}
                              />
                              <div className="flex gap-2">
                                <Button variant="outline" className="flex-1">
                                  <Camera className="h-4 w-4 mr-2" />
                                  Photo
                                </Button>
                                <Button 
                                  className="flex-1 bg-green-600 hover:bg-green-700"
                                  onClick={() => updateStopStatus(stop.id, "delivered")}
                                >
                                  <FileSignature className="h-4 w-4 mr-2" />
                                  Sign & Complete
                                </Button>
                              </div>
                            </div>
                          )}
                          
                          {stop.status === "delivered" && stop.signature && (
                            <div className="p-3 bg-green-50 rounded">
                              <div className="flex items-center gap-2 text-green-700 mb-2">
                                <CheckCircle className="h-4 w-4" />
                                <span className="font-medium">Delivered Successfully</span>
                              </div>
                              <p className="text-sm text-green-600">
                                Signed by: {stop.signature}
                              </p>
                              {stop.deliveryNotes && (
                                <p className="text-sm text-gray-600 mt-1">
                                  Notes: {stop.deliveryNotes}
                                </p>
                              )}
                            </div>
                          )}
                        </div>
                      </div>
                    </DialogContent>
                  </Dialog>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Bottom Actions */}
      <div className="sticky bottom-0 bg-white border-t p-4">
        {completedStops === totalStops ? (
          <Button className="w-full bg-green-600 hover:bg-green-700" size="lg">
            <CheckCircle className="h-5 w-5 mr-2" />
            Complete Load & Return
          </Button>
        ) : (
          <div className="text-center text-gray-600">
            <p className="text-sm">
              {totalStops - completedStops} stops remaining
            </p>
          </div>
        )}
      </div>
    </div>
  );
}