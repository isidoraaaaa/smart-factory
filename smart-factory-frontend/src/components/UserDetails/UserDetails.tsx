import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useAuth } from "../../context/useAuth";
import type { User } from "../../types/User";
import { getUser } from "../../services/usersService";
import "./UserDetails.css";

export function UserDetails() {
  const { userId } = useParams<{ userId: string }>();
  const navigate = useNavigate();
  const { token } = useAuth();
  const [username, setUsername] = useState("");
  const [name, setName] = useState("");
  const [lastname, setLastname] = useState("");
  const [email, setEmail] = useState("");
  const [userType, setUserType] = useState("");

  useEffect(() => {
    if (!userId) return;

    const loadInitialData = async () => {
      try {
        const user: User | null = await getUser(token, userId);

        if (user == null) {
          navigate("/");
          return;
        }

        setUsername(user.username);
        setName(user.name);
        setLastname(user.lastname);
        setEmail(user.email);
        setUserType(user.userType);
      } catch (err) {
        console.error("Failed to load user details:", err);
      }
    };

    loadInitialData();
  }, [token, navigate, userId]);

  return (
    <div className="user-details-container">
      <div className="user-details-card">
        <h1>User Details</h1>

        <div className="user-detail">
          <span>Name</span>
          <p>{name}</p>
        </div>

        <div className="user-detail">
          <span>Lastname</span>
          <p>{lastname}</p>
        </div>

        <div className="user-detail">
          <span>Email</span>
          <p>{email}</p>
        </div>

        <div className="user-detail">
          <span>Username</span>
          <p>{username}</p>
        </div>

        <div className="user-detail">
          <span>Role</span>
          <p>{userType}</p>
        </div>
      </div>
    </div>
  );
}
