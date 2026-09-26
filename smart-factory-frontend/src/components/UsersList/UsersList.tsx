import { useNavigate } from "react-router-dom";
import { useAuth } from "../../context/useAuth";
import { useEffect, useState } from "react";
import type { User } from "../../types/User";
import { deleteUser, getUsers } from "../../services/usersService";
import "./UsersList.css";

export function UsersList() {
  const { token } = useAuth();
  const navigate = useNavigate();

  const [users, setUsers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchUsers = async () => {
    setIsLoading(true);
    try {
      const data: User[] = await getUsers(token);
      setUsers(data);
    } catch (err) {
      console.error("Failed to load users:", err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleDeleteUser = async (id: string) => {
    try {
      await deleteUser(token, id);
      await fetchUsers();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong.");
    }
  };

  useEffect(() => {
    fetchUsers();
  }, [token]);

  return (
    <div className="users-list-container">
      <div className="users-list-header">
        <h1>Users</h1>
      </div>

      {error && <p className="users-list-error">{error}</p>}

      {isLoading ? (
        <p className="users-list-info">Loading users...</p>
      ) : users.length === 0 ? (
        <p className="users-list-info">No users were found.</p>
      ) : (
        <ul className="users-list">
          {users.map((user) => (
            <li key={user.id} className="user-item">
              <div
                className="user-item-info"
                onClick={() => navigate(`/users/${user.id}`)}
              >
                <span className="user-item-name">{user.name}</span>
                <span className={`user-item-usertype `}>{user.userType}</span>
              </div>

              <button
                className="delete-user-button"
                onClick={() => handleDeleteUser(user.id)}
              >
                Delete
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
